using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace API_SISTEMA.Utilidades;

public static class HuellaOperacion
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new DecimalCanonico() } };
    public static string Calcular<T>(T solicitud) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(solicitud, Options)));
    private sealed class DecimalCanonico : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => reader.GetDecimal();
        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            => writer.WriteRawValue(value.ToString("G29", CultureInfo.InvariantCulture));
    }
}
