using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.WhatsApp;

public sealed class WhatsAppWebhookService(SistemaDbContext db, IOptions<WhatsAppOptions> options)
{
    public static bool ValidarFirma(ReadOnlySpan<byte> cuerpo, string? firma, string secreto)
    {
        if (string.IsNullOrWhiteSpace(secreto) || firma is null || firma.Length != 71 || !firma.StartsWith("sha256=")) return false;
        try
        {
            var recibida = Convert.FromHexString(firma[7..]);
            var calculada = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secreto), cuerpo);
            return CryptographicOperations.FixedTimeEquals(calculada, recibida);
        }
        catch (FormatException) { return false; }
    }

    public static bool TokensIguales(string? recibido, string esperado) => !string.IsNullOrWhiteSpace(esperado) && recibido is not null &&
        CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(recibido)), SHA256.HashData(Encoding.UTF8.GetBytes(esperado)));

    public async Task Guardar(byte[] cuerpo, CancellationToken ct)
    {
        var config = options.Value;
        using var json = JsonDocument.Parse(cuerpo);
        var root = json.RootElement;
        if (Texto(root, "object") != "whatsapp_business_account") return;
        var eventos = new Dictionary<string, WhatsAppWebhookEvento>();
        foreach (var entry in Lista(root, "entry"))
        {
            if (Texto(entry, "id") != config.BusinessAccountId) continue;
            foreach (var change in Lista(entry, "changes"))
            {
                if (Texto(change, "field") != "messages" || !change.TryGetProperty("value", out var value) ||
                    value.ValueKind != JsonValueKind.Object || !value.TryGetProperty("metadata", out var metadata) ||
                    Texto(metadata, "phone_number_id") != config.PhoneNumberId) continue;
                foreach (var status in Lista(value, "statuses"))
                {
                    var id = Texto(status, "id"); var estado = Texto(status, "status");
                    if (string.IsNullOrEmpty(id) || id.Length > 512 || estado is not ("sent" or "delivered" or "read" or "failed") ||
                        !long.TryParse(Texto(status, "timestamp"), NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp) || timestamp < 0) continue;
                    int? codigo = null;
                    foreach (var error in Lista(status, "errors"))
                        if (error.ValueKind == JsonValueKind.Object && error.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number && code.TryGetInt32(out var numero)) { codigo = numero; break; }
                    var clave = JsonSerializer.Serialize(new { config.BusinessAccountId, config.PhoneNumberId, id, estado, timestamp, codigo });
                    var huella = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(clave)));
                    eventos[huella] = new() { huella = huella, cuenta = config.BusinessAccountId, remitente = config.PhoneNumberId,
                        message_id = id, estado = estado, timestamp_meta = timestamp, codigo_meta = codigo, recibido_utc = DateTime.UtcNow };
                }
            }
        }
        if (eventos.Count == 0) return;
        // Guardar antes del 200; si SQL falla, Meta puede reintentar. La PK deduplica reentregas concurrentes.
        await using var transaccion = await db.Database.BeginTransactionAsync(ct);
        foreach (var evento in eventos.Values)
            if (!await db.WhatsAppWebhookEventos.AnyAsync(e => e.huella == evento.huella, ct)) db.WhatsAppWebhookEventos.Add(evento);
        await db.SaveChangesAsync(ct);
        await transaccion.CommitAsync(ct);
    }

    private static string? Texto(JsonElement elemento, string nombre) => elemento.ValueKind == JsonValueKind.Object &&
        elemento.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.String ? valor.GetString() : null;
    private static IEnumerable<JsonElement> Lista(JsonElement elemento, string nombre) => elemento.ValueKind == JsonValueKind.Object &&
        elemento.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.Array ? valor.EnumerateArray() : [];
}

public static class WhatsAppEntrega
{
    public static WhatsAppWebhookEvento? Ultimo(IEnumerable<WhatsAppWebhookEvento> eventos)
    {
        // Un sent/failed tardío no puede hacer retroceder una confirmación de entrega o lectura.
        return eventos.OrderByDescending(e => e.estado == "read" ? 3 : e.estado == "delivered" ? 2 : 1)
            .ThenByDescending(e => e.timestamp_meta).ThenByDescending(e => e.estado == "failed").FirstOrDefault();
    }

    public static string? Mostrar(WhatsAppWebhookEvento? evento) => evento?.estado switch {
        "sent" => "enviado", "delivered" => "entregado", "read" => "leido", "failed" => "fallido", _ => null
    };
}
