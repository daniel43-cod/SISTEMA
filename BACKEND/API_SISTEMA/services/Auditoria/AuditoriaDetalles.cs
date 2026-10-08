using System.Text.Json;
using API_SISTEMA.Models;

namespace API_SISTEMA.Services.Auditoria;

internal static class AuditoriaDetalles
{
    // Solo se reciben objetos con campos permitidos, nunca entidades completas.
    public static List<AuditoriaEventoDetalle> Crear(object? anteriores, object nuevos)
    {
        var antes = anteriores is null ? (JsonElement?)null : JsonSerializer.SerializeToElement(anteriores);
        var despues = JsonSerializer.SerializeToElement(nuevos);
        var detalles = new List<AuditoriaEventoDetalle>();
        foreach (var campo in despues.EnumerateObject())
        {
            JsonElement anterior = default;
            var existe = antes.HasValue && antes.Value.TryGetProperty(campo.Name, out anterior);
            var valorAnterior = existe ? Valor(anterior) : null;
            var valorNuevo = Valor(campo.Value);
            if (anteriores is not null && existe && valorAnterior == valorNuevo) continue;
            detalles.Add(new AuditoriaEventoDetalle
            {
                Campo = campo.Name, ValorAnterior = valorAnterior, ValorNuevo = valorNuevo
            });
        }
        return detalles;
    }

    private static string? Valor(JsonElement valor) => valor.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => valor.GetString(),
        _ => valor.GetRawText()
    };
}
