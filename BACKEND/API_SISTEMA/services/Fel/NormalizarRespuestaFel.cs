using System.Globalization;
using System.Text.Json;
using API_SISTEMA.Models;

namespace API_SISTEMA.Services.Fel;

public static class NormalizarRespuestaFel
{
    private static JsonElement Campo(JsonElement e, string nombre) => e.ValueKind == JsonValueKind.Object &&
        e.TryGetProperty(nombre, out var valor) ? valor : default;
    private static string? Texto(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(), JsonValueKind.Number => e.GetRawText(), _ => null
    };
    private static string? Texto(JsonElement e, string nombre) => Texto(Campo(e, nombre));
    private static bool? Booleano(JsonElement e, string nombre) => Campo(e, nombre).ValueKind switch
    {
        JsonValueKind.True => true, JsonValueKind.False => false, _ => null
    };
    private static long? Entero(JsonElement e, string nombre) => long.TryParse(Texto(e, nombre),
        NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    public static void Aplicar(DocumentoFelIntento intento, InfileRespuesta respuesta)
    {
        intento.codigo_http = (short)respuesta.EstadoHttp;
        intento.resultado = respuesta.Resultado;
        intento.uuid_respuesta = respuesta.Uuid;
        intento.serie_respuesta = respuesta.Serie;
        intento.numero_respuesta = respuesta.Numero;
        intento.fecha_respuesta = DateTimeOffset.UtcNow;
        try
        {
            using var json = JsonDocument.Parse(respuesta.Json);
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new JsonException();
            intento.origen = Texto(root, "origen");
            intento.descripcion = Texto(root, "descripcion");
            if (DateTimeOffset.TryParse(Texto(root, "fecha"), CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fecha)) intento.fecha_respuesta = fecha;
            var control = Campo(root, "control_emision");
            intento.saldo = Entero(root, "saldo") ?? Entero(control, "Saldo");
            intento.creditos = Entero(root, "creditos") ?? Entero(control, "Creditos");
            intento.cupo_diario = Entero(control, "cupo_diario");
            intento.saldo_diario = Entero(control, "saldo_diario");
            intento.alertas_infile = Booleano(root, "alertas_infile");
            intento.alertas_sat = Booleano(root, "alertas_sat");
            var cantidad = Entero(root, "cantidad_errores");
            intento.cantidad_errores = cantidad is >= 0 and <= int.MaxValue ? (int?)cantidad : null;
            var errores = Campo(root, "descripcion_errores");
            if (errores.ValueKind == JsonValueKind.Array)
            {
                var orden = 0;
                foreach (var e in errores.EnumerateArray()) intento.Errores.Add(new()
                {
                    orden = orden++, resultado = Booleano(e, "resultado"), fuente = Texto(e, "fuente"),
                    categoria = Texto(e, "categoria"), numeral = Texto(e, "numeral"), validacion = Texto(e, "validacion"),
                    mensaje = Texto(e) ?? Texto(e, "mensaje_error") ?? Texto(e, "mensaje") ?? Texto(e, "descripcion")
                });
            }
            foreach (var origen in new[] { "INFILE", "SAT" })
            {
                var alertas = Campo(root, "descripcion_alertas_" + origen.ToLowerInvariant());
                if (alertas.ValueKind != JsonValueKind.Array) continue;
                var orden = 0;
                foreach (var a in alertas.EnumerateArray()) intento.Alertas.Add(new()
                {
                    origen = origen, orden = orden++, fuente = Texto(a, "fuente"), categoria = Texto(a, "categoria"),
                    numeral = Texto(a, "numeral"), validacion = Texto(a, "validacion"),
                    mensaje = Texto(a) ?? Texto(a, "mensaje_error") ?? Texto(a, "mensaje") ?? Texto(a, "descripcion")
                });
            }
        }
        catch (JsonException)
        {
            intento.descripcion = "INFILE devolvió una respuesta que no es un objeto JSON válido. Se requiere confirmar el resultado.";
        }
    }
}
