using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Comprobantes;

namespace API_SISTEMA.Services.WhatsApp;

public sealed record PaginaWhatsAppResultado(int Pagina, string Estado, string? MessageId = null,
    string? Etapa = null, int? CodigoMeta = null, string? EstadoEntrega = null, int? CodigoEntrega = null);
public sealed record EnvioWhatsAppResultado(int IdVenta, string Estado, int TotalPaginas,
    IReadOnlyList<PaginaWhatsAppResultado> Paginas);

public sealed class EnviarComprobanteWhatsAppService(WhatsAppClient cliente)
{
    public static string NormalizarTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) throw new ValidationException("El cliente no tiene teléfono registrado.");
        var normalizado = Regex.Replace(telefono.Trim(), @"[\s()\-]", "");
        if (normalizado.StartsWith('+')) normalizado = normalizado[1..];
        if (Regex.IsMatch(normalizado, @"^[0-9]{8}$")) normalizado = "502" + normalizado;
        if (!Regex.IsMatch(normalizado, @"^[1-9][0-9]{8,14}$"))
            throw new ValidationException("El teléfono del cliente no es válido. Corrige el dato del cliente antes de enviar.");
        return normalizado;
    }

    public async Task<EnvioWhatsAppResultado> Enviar(Models.Ventas venta, Empresa empresa, CancellationToken ct)
    {
        var telefono = NormalizarTelefono(venta.cliente.telefono);
        cliente.ValidarConfiguracion();
        var paginas = ComprobanteImagen.Planificar(venta, empresa);
        var resultados = new List<PaginaWhatsAppResultado>();
        bool detenido = false;
        for (int i = 1; i <= paginas.Count; i++)
        {
            if (detenido || ct.IsCancellationRequested)
            {
                detenido = true;
                resultados.Add(new(i, "no_intentado"));
                continue;
            }
            byte[] png;
            try { png = ComprobanteImagen.Generar(venta, empresa, paginas, i); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or ValidationException)
            {
                resultados.Add(new(i, "fallido", Etapa: "generacion"));
                detenido = true;
                continue;
            }
            try
            {
                var mediaId = await cliente.SubirImagen(png, ct);
                var messageId = await cliente.EnviarPlantilla(telefono, mediaId, ct);
                resultados.Add(new(i, "aceptado", messageId));
            }
            catch (WhatsAppApiException ex)
            {
                resultados.Add(new(i, ex.Estado, Etapa: ex.Etapa, CodigoMeta: ex.Codigo));
                detenido = true;
            }
            catch (ValidationException)
            {
                resultados.Add(new(i, "fallido", Etapa: "subida"));
                detenido = true;
            }
        }
        var estado = resultados.All(r => r.Estado == "aceptado") ? "aceptado"
            : resultados.Any(r => r.Estado == "aceptado") ? "parcial"
            : resultados.Any(r => r.Estado == "desconocido") ? "desconocido" : "fallido";
        return new(venta.id_ventas, estado, paginas.Count, resultados);
    }
}
