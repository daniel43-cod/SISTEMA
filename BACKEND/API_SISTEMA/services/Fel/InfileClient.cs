using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.Fel;

// Los fallos de transporte no prueban que INFILE haya rechazado el documento.
// El servicio de certificación deberá resolverlos con el mismo identificador y XML.
public sealed record InfileRespuesta(HttpStatusCode EstadoHttp, string Json, bool? Resultado,
    string? Uuid, string? Serie, string? Numero, string? XmlCertificadoBase64);

public sealed class InfileClient(HttpClient http, IOptions<InfileOptions> options)
{
    public async Task<InfileRespuesta> EnviarAsync(string xml, Guid identificador, CancellationToken ct = default)
    {
        var config = options.Value;
        if (!config.Habilitado) throw new InvalidOperationException("La integración FEL está deshabilitada.");
        if (identificador == Guid.Empty) throw new ArgumentException("El identificador es obligatorio.", nameof(identificador));
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var request = new HttpRequestMessage(HttpMethod.Post,
            "https://certificador.feel.com.gt/fel/procesounificado/transaccion/v2/xml");
        request.Content = new StringContent(xml, Encoding.UTF8, "application/xml");
        request.Headers.Add("UsuarioAPI", config.UsuarioApi);
        request.Headers.Add("LlaveAPI", config.LlaveApi);
        request.Headers.Add("UsuarioFirma", config.UsuarioFirma);
        request.Headers.Add("LlaveFirma", config.LlaveFirma);
        request.Headers.Add("Identificador", identificador.ToString());

        // No hay reintentos automáticos: primero se debe persistir la operación.
        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            string? Texto(string nombre) => root.TryGetProperty(nombre, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() : null;
            bool? resultado = root.TryGetProperty("resultado", out var value) &&
                value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : null;
            var numero = Texto("numero");
            if (numero is null && root.TryGetProperty("numero", out var numeroJson) &&
                numeroJson.ValueKind == JsonValueKind.Number && numeroJson.TryGetInt64(out var numeroEntero))
                numero = numeroEntero.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return new(response.StatusCode, body, resultado, Texto("uuid"), Texto("serie"),
                numero, Texto("xml_certificado"));
        }
        catch (JsonException)
        {
            return new(response.StatusCode, body, null, null, null, null, null);
        }
        catch (InvalidOperationException)
        {
            return new(response.StatusCode, body, null, null, null, null, null);
        }
    }
}
