using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.WhatsApp;

// Nunca se expone el cuerpo de error de Meta: podría incluir datos privados.
public sealed class WhatsAppApiException(string etapa, string estado, int? codigo = null) : Exception("No se pudo completar la operación con WhatsApp.")
{
    public string Etapa { get; } = etapa;
    public string Estado { get; } = estado;
    public int? Codigo { get; } = codigo;
}

public sealed class WhatsAppClient(HttpClient http, IOptions<WhatsAppOptions> options)
{
    public void ValidarConfiguracion() => options.Value.Validar();

    public async Task<string> SubirImagen(byte[] png, CancellationToken ct)
    {
        if (png.Length == 0 || png.Length > 5 * 1024 * 1024)
            throw new System.ComponentModel.DataAnnotations.ValidationException("La imagen debe pesar entre 1 byte y 5 MB.");
        using var contenido = new MultipartFormDataContent();
        contenido.Add(new StringContent("whatsapp"), "messaging_product");
        contenido.Add(new StringContent("image/png"), "type");
        var archivo = new ByteArrayContent(png);
        archivo.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        contenido.Add(archivo, "file", "comprobante.png");
        return await Solicitar("media", contenido, "subida", ct);
    }

    public Task<string> EnviarPlantilla(string telefono, string mediaId, CancellationToken ct)
    {
        var config = options.Value;
        var contenido = JsonContent.Create(new {
            messaging_product = "whatsapp", recipient_type = "individual", to = telefono, type = "template",
            template = new { name = config.TemplateName, language = new { code = config.TemplateLanguage },
                components = new[] { new { type = "header", parameters = new[] {
                    new { type = "image", image = new { id = mediaId } }
                } } } }
        });
        return Solicitar("messages", contenido, "envio", ct);
    }

    private async Task<string> Solicitar(string recurso, HttpContent contenido, string etapa, CancellationToken ct)
    {
        using (contenido)
        {
            ValidarConfiguracion();
            var config = options.Value;
            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://graph.facebook.com/{config.ApiVersion}/{config.PhoneNumberId}/{recurso}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.AccessToken);
            request.Content = contenido;
            // No se reintenta: perder una respuesta no demuestra que el mensaje no se envió.
            try
            {
                using var response = await http.SendAsync(request, ct);
                var texto = await response.Content.ReadAsStringAsync(ct);
                if (!response.IsSuccessStatusCode)
                {
                    int? codigo = null;
                    try
                    {
                        using var error = JsonDocument.Parse(texto);
                        if (error.RootElement.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.Object &&
                            e.TryGetProperty("code", out var c) && c.TryGetInt32(out var n)) codigo = n;
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidOperationException) { }
                    throw new WhatsAppApiException(etapa,
                        etapa == "envio" && (int)response.StatusCode >= 500 ? "desconocido" : "rechazado", codigo);
                }
                using var document = JsonDocument.Parse(texto);
                var root = document.RootElement;
                string? id = null;
                if (recurso == "media" && root.TryGetProperty("id", out var media) && media.ValueKind == JsonValueKind.String)
                    id = media.GetString();
                if (recurso == "messages" && root.TryGetProperty("messages", out var mensajes) &&
                    mensajes.ValueKind == JsonValueKind.Array && mensajes.GetArrayLength() > 0 &&
                    mensajes[0].TryGetProperty("id", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
                    id = mensaje.GetString();
                if (string.IsNullOrWhiteSpace(id)) throw new JsonException();
                return id;
            }
            catch (WhatsAppApiException) { throw; }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidOperationException)
            {
                throw new WhatsAppApiException(etapa, etapa == "envio" ? "desconocido" : "fallido");
            }
        }
    }
}
