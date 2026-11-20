using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.Fel;

public sealed record ConsultaReceptorResultado(bool resultado, string tipo, string identificacion,
    string? nombre, string? estatus, string? descripcion);
public sealed class ConsultaReceptorNoDisponibleException : Exception;

// El token de INFILE se comparte entre solicitudes; no es el JWT de usuarios del sistema.
public sealed class ConsultarReceptorInfileService(IHttpClientFactory factory, IOptions<InfileOptions> options, TimeProvider reloj)
{
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private string? token;
    private DateTimeOffset vencimiento;

    public async Task<ConsultaReceptorResultado> Consultar(string tipo, string identificacion, CancellationToken ct = default)
    {
        tipo = tipo?.Trim().ToUpperInvariant() ?? "";
        if (tipo is not ("NIT" or "CUI")) throw new ValidationException("Selecciona NIT o CUI para consultar al receptor.");
        var normalizado = IdentificacionReceptorFel.Resolver(tipo == "NIT" ? identificacion : null, tipo == "CUI" ? identificacion : null, 0m);
        if (normalizado.Identificacion == "CF") throw new ValidationException("Consumidor final no requiere consulta de documento.");
        if (!options.Value.Habilitado) throw new ValidationException("La integración FEL está deshabilitada.");
        try
        {
            JsonElement respuesta;
            if (tipo == "NIT")
            {
                using var client = factory.CreateClient("infile-receptores");
                using var http = await client.PostAsJsonAsync("https://consultareceptores.feel.com.gt/rest/action", new
                {
                    emisor_codigo = options.Value.UsuarioApi, emisor_clave = options.Value.LlaveApi,
                    nit_consulta = normalizado.Identificacion
                }, ct);
                respuesta = await Leer(http, ct);
            }
            else
            {
                respuesta = default;
                for (var intento = 0; intento < 2; intento++)
                {
                    var jwt = await ObtenerToken(ct);
                    using var client = factory.CreateClient("infile-receptores");
                    using var request = new HttpRequestMessage(HttpMethod.Post, "https://certificador.feel.com.gt/api/v2/servicios/externos/cui");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                    request.Content = new MultipartFormDataContent { { new StringContent(normalizado.Identificacion), "cui" } };
                    using var http = await client.SendAsync(request, ct);
                    if (http.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    {
                        await InvalidarToken(jwt, ct);
                        if (intento == 0) continue;
                        throw new ConsultaReceptorNoDisponibleException();
                    }
                    respuesta = await Leer(http, ct);
                    var descripcion = Texto(respuesta, "descripcion")?.ToLowerInvariant() ?? "";
                    if (!Exito(respuesta) && descripcion.Contains("token") &&
                        (descripcion.Contains("expir") || descripcion.Contains("inválid") || descripcion.Contains("invalid")))
                    {
                        await InvalidarToken(jwt, ct);
                        if (intento == 0) continue;
                        throw new ConsultaReceptorNoDisponibleException();
                    }
                    break;
                }
            }
            var datos = respuesta;
            var esCuiAnidado = tipo == "CUI" && respuesta.TryGetProperty("cui", out datos) && datos.ValueKind == JsonValueKind.Object;
            if (!esCuiAnidado) datos = respuesta;
            var nombre = (tipo == "NIT" ? Texto(datos, "razon_social") ?? Texto(datos, "nombre") : Texto(datos, "nombre"))?.Trim();
            var estatus = Texto(datos, "estatus")?.Trim().ToUpperInvariant();
            var existeResultado = respuesta.TryGetProperty("resultado", out _);
            var exitoso = Exito(respuesta);
            if (tipo == "NIT" && !existeResultado)
            {
                // Formato real: { nit, nombre, mensaje }, sin bandera resultado.
                var nitRetornado = Texto(respuesta, "nit")?.Replace("-", "").Trim().ToUpperInvariant();
                exitoso = nitRetornado == normalizado.Identificacion && !string.IsNullOrWhiteSpace(nombre)
                    && string.IsNullOrWhiteSpace(Texto(respuesta, "mensaje"));
            }
            if (esCuiAnidado)
            {
                var cuiRetornado = Texto(datos, "cui")?.Trim();
                exitoso = exitoso && cuiRetornado == normalizado.Identificacion;
                var fallecido = Texto(datos, "fallecido")?.Trim().ToUpperInvariant();
                estatus = fallecido is "NO" or "N" or "0" or "FALSE" ? "ACTIVO"
                    : fallecido is "SI" or "SÍ" or "S" or "1" or "TRUE" ? "FALLECIDO" : null;
            }
            if (!exitoso || string.IsNullOrWhiteSpace(nombre) || (tipo == "CUI" && estatus != "ACTIVO"))
                return new(false, tipo, normalizado.Identificacion, null, estatus,
                    tipo == "NIT" ? "INFILE no devolvió una razón social válida para ese NIT. Verifica el documento o consulta a soporte de INFILE."
                    : "INFILE no devolvió un CUI activo con nombre registrado. Verifica el documento.");
            if (nombre.Length > 250) throw new ConsultaReceptorNoDisponibleException();
            return new(true, tipo, normalizado.Identificacion, nombre, estatus, null);
        }
        catch (Exception e) when (e is HttpRequestException or JsonException || e is OperationCanceledException && !ct.IsCancellationRequested)
        {
            throw new ConsultaReceptorNoDisponibleException();
        }
    }

    private async Task<string> ObtenerToken(CancellationToken ct)
    {
        await tokenLock.WaitAsync(ct);
        try
        {
            if (token is not null && reloj.GetUtcNow() < vencimiento) return token;
            using var client = factory.CreateClient("infile-receptores");
            using var form = new MultipartFormDataContent
            {
                { new StringContent(options.Value.UsuarioApi), "prefijo" },
                { new StringContent(options.Value.LlaveApi), "llave" }
            };
            using var http = await client.PostAsync("https://certificador.feel.com.gt/api/v2/servicios/externos/login", form, ct);
            var respuesta = await Leer(http, ct);
            var nuevo = Texto(respuesta, "token");
            if (!Exito(respuesta) || string.IsNullOrWhiteSpace(nuevo)) throw new ConsultaReceptorNoDisponibleException();
            token = nuevo;
            vencimiento = reloj.GetUtcNow().AddMinutes(110); // La documentación establece dos horas; renovar con margen.
            return token;
        }
        finally { tokenLock.Release(); }
    }
    private async Task InvalidarToken(string anterior, CancellationToken ct)
    {
        await tokenLock.WaitAsync(ct);
        try { if (token == anterior) token = null; }
        finally { tokenLock.Release(); }
    }
    private static bool Exito(JsonElement e) => e.TryGetProperty("resultado", out var v) && v.ValueKind == JsonValueKind.True;
    private static string? Texto(JsonElement e, string campo) => e.TryGetProperty(campo, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static async Task<JsonElement> Leer(HttpResponseMessage http, CancellationToken ct)
    {
        if (!http.IsSuccessStatusCode) throw new ConsultaReceptorNoDisponibleException();
        using var json = JsonDocument.Parse(await http.Content.ReadAsStringAsync(ct));
        if (json.RootElement.ValueKind != JsonValueKind.Object) throw new JsonException();
        return json.RootElement.Clone();
    }
}
