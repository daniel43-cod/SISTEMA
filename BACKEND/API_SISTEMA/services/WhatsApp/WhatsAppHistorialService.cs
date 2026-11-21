using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Comprobantes;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.WhatsApp;

public sealed record WhatsAppHistorialResultado(Guid IdEnvio, EnvioWhatsAppResultado Resultado)
{
    public bool HuboFallo => Resultado.Estado is "fallido" or "parcial" or "desconocido" or "fallido_entrega" or "pendiente";
    public string Mensaje => Resultado.Estado switch {
        "leido" => "El comprobante fue leído por el cliente.",
        "entregado" => "El comprobante fue entregado al cliente.",
        "aceptado" => "El comprobante fue aceptado por WhatsApp. La entrega está pendiente de confirmación.",
        "en_proceso" => "El comprobante se está enviando.",
        _ => "Hubo un fallo al compartir el comprobante. Intenta nuevamente más tarde."
    };
}
public sealed record WhatsAppIntentoResultado(long IdIntento, int Pagina, int IdUsuario, DateTime InicioUtc,
    DateTime? FinUtc, string Estado, string? MessageId, string? Etapa, int? CodigoMeta);

public sealed class WhatsAppHistorialService(SistemaDbContext db, WhatsAppClient cliente, IOptions<WhatsAppOptions> options)
{
    // Cada llamada es una acción explícita de Compartir, incluso si la venta no cambió.
    public static string? TelefonoRegistrado(Models.Ventas venta) => venta.cliente.telefono;

    public static string ResolverTelefono(Models.Ventas venta, string? temporal = null)
    {
        var registrado = TelefonoRegistrado(venta);
        if (!string.IsNullOrWhiteSpace(registrado) && !string.IsNullOrWhiteSpace(temporal))
            throw new ValidationException("La venta ya tiene un teléfono registrado. Se debe usar ese número.");
        return EnviarComprobanteWhatsAppService.NormalizarTelefono(!string.IsNullOrWhiteSpace(registrado) ? registrado : temporal);
    }

    public async Task GuardarTelefonoRegistro(Models.Ventas venta, string telefonoIngresado, CancellationToken ct)
    {
        var telefono = ResolverTelefono(venta, telefonoIngresado);
        var actualizado = await db.cliente.Where(c => c.id_cliente == venta.id_cliente &&
                (c.telefono == null || c.telefono.Trim() == ""))
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.telefono, telefono), ct);
        if (actualizado != 1) throw new VentaConflictoException("Ya se registró un teléfono. Actualiza el comprobante para usarlo.");
        venta.cliente.telefono = telefono;
    }

    public async Task<WhatsAppHistorialResultado> Iniciar(Models.Ventas venta, Empresa empresa, int usuario, CancellationToken ct, string? telefonoTemporal = null)
    {
        return await IniciarImagenes(venta, empresa, usuario, ct, telefonoTemporal,
            ComprobanteImagen.Planificar(venta, empresa), false);
    }

    public async Task<WhatsAppHistorialResultado> IniciarPagos(Models.Ventas venta, Empresa empresa, int usuario,
        CancellationToken ct, string? telefonoTemporal = null)
    {
        var pagos = await new PagoService(db).ListarPorVenta(venta.id_ventas, ct);
        if (pagos.Count == 0) throw new ValidationException("Esta venta aún no tiene pagos registrados.");
        return await IniciarImagenes(venta, empresa, usuario, ct, telefonoTemporal,
            ComprobanteImagen.PlanificarPagos(venta, empresa, pagos), true);
    }

    private async Task<WhatsAppHistorialResultado> IniciarImagenes(Models.Ventas venta, Empresa empresa, int usuario,
        CancellationToken ct, string? telefonoTemporal, List<ComprobanteImagen.Pagina> paginas, bool historialPagos)
    {
        cliente.ValidarConfiguracion();
        var telefono = ResolverTelefono(venta, telefonoTemporal);
        var config = options.Value;
        var envio = new WhatsAppEnvio { id_envio = Guid.NewGuid(), id_venta = venta.id_ventas,
            id_empresa = empresa.id_empresa, id_usuario = usuario, telefono = telefono,
            plantilla = config.TemplateName, idioma = config.TemplateLanguage, remitente = config.PhoneNumberId,
            creado_utc = DateTime.UtcNow,
            paginas = Enumerable.Range(1, paginas.Count).Select(p => new WhatsAppEnvioPagina { pagina = p }).ToList() };
        db.WhatsAppEnvios.Add(envio);
        await db.SaveChangesAsync(ct);
        foreach (var pagina in envio.paginas.OrderBy(p => p.pagina))
        {
            if (ct.IsCancellationRequested) break;
            var intento = new WhatsAppEnvioIntento { id_envio = envio.id_envio, pagina = pagina.pagina, id_usuario = usuario,
                inicio_utc = DateTime.UtcNow, estado = "subiendo", etapa = "generacion" };
            db.WhatsAppEnvioIntentos.Add(intento);
            pagina.estado = "subiendo"; pagina.etapa = "generacion"; pagina.actualizado_utc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            try
            {
                // Se usan datos actuales en memoria; no se guardan PNG ni copias JSON.
                var png = ComprobanteImagen.Generar(venta, empresa, paginas, pagina.pagina, historialPagos);
                pagina.etapa = intento.etapa = "subida";
                pagina.actualizado_utc = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                var mediaId = await cliente.SubirImagen(png, ct);
                pagina.estado = intento.estado = "enviando";
                pagina.etapa = intento.etapa = "envio";
                pagina.actualizado_utc = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                var messageId = await cliente.EnviarPlantilla(telefono, mediaId, ct);
                pagina.estado = intento.estado = "aceptado";
                pagina.message_id = intento.message_id = messageId;
                pagina.etapa = intento.etapa = null;
            }
            catch (WhatsAppApiException ex)
            {
                pagina.estado = intento.estado = ex.Estado;
                pagina.etapa = intento.etapa = ex.Etapa;
                pagina.codigo_meta = intento.codigo_meta = ex.Codigo;
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException or InvalidOperationException)
            {
                if (ex is InvalidOperationException && pagina.etapa != "generacion") throw;
                pagina.estado = intento.estado = "fallido";
            }
            catch (OperationCanceledException)
            {
                pagina.estado = intento.estado = "fallido";
            }
            intento.fin_utc = pagina.actualizado_utc = DateTime.UtcNow;
            using var guardar = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await db.SaveChangesAsync(guardar.Token);
            if (pagina.estado != "aceptado") break;
        }
        return await Consultar(envio.id_envio, CancellationToken.None);
    }

    public async Task<WhatsAppHistorialResultado> Consultar(Guid idEnvio, CancellationToken ct)
    {
        var envio = await db.WhatsAppEnvios.AsNoTracking().Include(e => e.paginas)
            .SingleOrDefaultAsync(e => e.id_envio == idEnvio, ct)
            ?? throw new KeyNotFoundException("El envío no existe.");
        var ids = envio.paginas.Where(p => p.message_id != null).Select(p => p.message_id!).ToArray();
        var eventos = await db.WhatsAppWebhookEventos.AsNoTracking()
            .Where(e => e.remitente == envio.remitente && ids.Contains(e.message_id)).ToListAsync(ct);
        var paginas = envio.paginas.OrderBy(p => p.pagina).Select(p => {
            var entrega = WhatsAppEntrega.Ultimo(eventos.Where(e => e.message_id == p.message_id));
            var estadoPagina = p.estado is "subiendo" or "enviando" &&
                DateTime.UtcNow - (p.actualizado_utc ?? envio.creado_utc) > TimeSpan.FromMinutes(10) ? "desconocido" : p.estado;
            return new PaginaWhatsAppResultado(p.pagina, estadoPagina, p.message_id, p.etapa, p.codigo_meta,
                WhatsAppEntrega.Mostrar(entrega), entrega?.codigo_meta);
        }).ToList();
        var estado = paginas.All(p => p.EstadoEntrega == "leido") ? "leido"
            : paginas.All(p => p.EstadoEntrega is "entregado" or "leido") ? "entregado"
            : paginas.Any(p => p.EstadoEntrega == "fallido") ? "fallido_entrega"
            : paginas.All(p => p.Estado == "aceptado") ? "aceptado"
            : paginas.Any(p => p.Estado is "subiendo" or "enviando") ? "en_proceso"
            : paginas.Any(p => p.Estado == "aceptado") ? "parcial"
            : paginas.Any(p => p.Estado == "desconocido") ? "desconocido"
            : paginas.All(p => p.Estado == "no_intentado") ? "pendiente" : "fallido";
        return new(idEnvio, new(envio.id_venta, estado, paginas.Count, paginas));
    }

    public async Task<object> Historial(int idVenta, int idEmpresa, CancellationToken ct, int pagina = 1, int tamanoPagina = 20)
    {
        if (idEmpresa < 1 || pagina < 1 || tamanoPagina is < 1 or > 50)
            throw new ValidationException("Selecciona una empresa, página positiva y tamaño entre 1 y 50.");
        var consulta = db.WhatsAppEnvios.AsNoTracking().Where(e => e.id_venta == idVenta && e.id_empresa == idEmpresa);
        var total = await consulta.CountAsync(ct);
        var salto = ((long)pagina - 1) * tamanoPagina;
        if (salto > int.MaxValue) throw new ValidationException("La página solicitada es demasiado grande.");
        var ids = await consulta.OrderByDescending(e => e.creado_utc).ThenByDescending(e => e.id_envio)
            .Skip((int)salto).Take(tamanoPagina).Select(e => e.id_envio).ToListAsync(ct);
        var envios = new List<object>();
        foreach (var id in ids)
        {
            var resumen = await Consultar(id, ct);
            var intentos = await db.WhatsAppEnvioIntentos.AsNoTracking().Where(i => i.id_envio == id)
                .OrderBy(i => i.id_intento).Select(i => new WhatsAppIntentoResultado(i.id_intento, i.pagina,
                    i.id_usuario, i.inicio_utc, i.fin_utc, i.estado, i.message_id, i.etapa, i.codigo_meta)).ToListAsync(ct);
            envios.Add(new { resumen.IdEnvio, resumen.Resultado, resumen.HuboFallo, resumen.Mensaje, intentos });
        }
        return new { pagina, tamanoPagina, totalEnvios = total, envios };
    }
}
