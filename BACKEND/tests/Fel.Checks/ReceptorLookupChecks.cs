using System.Net;
using System.Text.Json;
using API_SISTEMA.Services.Fel;
using Microsoft.Extensions.Options;

internal static class ReceptorLookupChecks
{
    public static async Task Run()
    {
        using var handler = new ReceptorHandler();
        var clock = new Reloj();
        var servicio = new ConsultarReceptorInfileService(new Factory(handler), Options.Create(new InfileOptions
        { Habilitado = true, UsuarioApi = "usuario-prueba", LlaveApi = "llave-prueba" }), clock);
        var nit = await servicio.Consultar("NIT", "1170142-k");
        Check(nit.resultado && nit.nombre == "NEGOCIO DEMO" && nit.identificacion == "1170142K", "NIT normalizado y razón social");
        handler.FormatoReal = true;
        Check((await servicio.Consultar("NIT", "1170142K")).nombre == "NEGOCIO DEMO", "NIT real sin bandera resultado");
        Check((await servicio.Consultar("CUI", "2993930620101")).nombre == "PERSONA DEMO", "Nombre dentro de objeto cui");
        handler.FormatoReal = false;
        var consultas = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => servicio.Consultar("CUI", "2993930620101")));
        Check(consultas.All(c => c.nombre == "PERSONA DEMO") && handler.Logins == 1, "JWT compartido en consultas concurrentes");
        handler.Expirar = true;
        await servicio.Consultar("CUI", "2993930620101");
        Check(handler.Logins == 2, "Renovación después de HTTP 401");
        clock.Ahora = clock.Ahora.AddHours(2);
        await servicio.Consultar("CUI", "2993930620101");
        Check(handler.Logins == 3, "Renovación antes de reutilizar token vencido");
        handler.NoEncontrado = true;
        var inexistente = await servicio.Consultar("NIT", "1170142K");
        Check(!inexistente.resultado && inexistente.nombre is null, "NIT no encontrado sin nombre inventado");
        handler.Fallecido = true;
        Check(!(await servicio.Consultar("CUI", "2993930620101")).resultado, "CUI no activo rechazado");
        var llamadas = handler.Llamadas;
        try { await servicio.Consultar("CUI", "123"); throw new Exception("CUI inválido aceptado."); }
        catch (System.ComponentModel.DataAnnotations.ValidationException) { }
        Check(handler.Llamadas == llamadas, "Formato inválido no consume INFILE");
        handler.RespuestaInvalida = true;
        try { await servicio.Consultar("NIT", "1170142K"); throw new Exception("Respuesta inválida aceptada."); }
        catch (ConsultaReceptorNoDisponibleException) { }
        handler.RespuestaInvalida = false;
        foreach (var cuerpo in new[]
        {
            "{\"resultado\":true,\"nit\":\"5203333\",\"razon_social\":\"OTRA PERSONA\"}",
            "{\"nit\":\"5203333\",\"nombre\":\"OTRA PERSONA\",\"mensaje\":\"\"}",
            "{\"resultado\":false,\"razon_social\":\"NOMBRE\"}",
            "{\"resultado\":\"true\",\"razon_social\":\"NOMBRE\"}",
            "{\"resultado\":true,\"razon_social\":\" \"}", "{}"
        })
        {
            handler.Cuerpo = cuerpo;
            Check(!(await servicio.Consultar("NIT", "1170142K")).resultado, "NIT con respuesta inconsistente debe rechazarse.");
        }
        foreach (var cuerpo in new[]
        {
            "{\"resultado\":true,\"cui\":{\"cui\":\"1234567890123\",\"nombre\":\"OTRA PERSONA\",\"fallecido\":\"NO\"}}",
            "{\"resultado\":true,\"cui\":{\"cui\":\"2993930620101\",\"nombre\":\"PERSONA\",\"fallecido\":\"SI\"}}",
            "{\"resultado\":true,\"cui\":{\"cui\":\"2993930620101\",\"nombre\":\"PERSONA\",\"fallecido\":\"DESCONOCIDO\"}}",
            "{\"resultado\":true,\"cui\":\"1234567890123\",\"nombre\":\"OTRA PERSONA\",\"estatus\":\"ACTIVO\"}",
            "{\"resultado\":true,\"nombre\":\"PERSONA\"}"
        })
        {
            handler.Cuerpo = cuerpo;
            Check(!(await servicio.Consultar("CUI", "2993930620101")).resultado, "CUI con respuesta inconsistente debe rechazarse.");
        }
        foreach (var cuerpo in new[] { "[]", "null", "true", "\"texto\"", "<html>Error</html>" })
        {
            handler.Cuerpo = cuerpo;
            try { await servicio.Consultar("NIT", "1170142K"); throw new Exception("Respuesta malformada aceptada."); }
            catch (ConsultaReceptorNoDisponibleException) { }
        }
        handler.Cuerpo = null;
        handler.NoAutorizado = true;
        var antes401 = handler.Llamadas;
        try { await servicio.Consultar("CUI", "2993930620101"); throw new Exception("Se aceptó un 401 persistente."); }
        catch (ConsultaReceptorNoDisponibleException) { }
        Check(handler.Llamadas - antes401 <= 4, "No repetir indefinidamente autenticación y consulta.");
        handler.NoAutorizado = false;
        using var cancelado = new CancellationTokenSource();
        cancelado.Cancel();
        try { await servicio.Consultar("CUI", "2993930620101", cancelado.Token); throw new Exception("Cancelación ignorada."); }
        catch (OperationCanceledException) { }
        Console.WriteLine("Consultas NIT/CUI: contratos HTTP, token concurrente, renovación y errores verificados con INFILE simulado.");
    }
    private static void Check(bool value, string mensaje) { if (!value) throw new Exception(mensaje); }
    private sealed class Reloj : TimeProvider
    {
        public DateTimeOffset Ahora = DateTimeOffset.Parse("2026-11-19T12:00:00-06:00");
        public override DateTimeOffset GetUtcNow() => Ahora;
    }
    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }
    private sealed class ReceptorHandler : HttpMessageHandler
    {
        public int Logins;
        public int Llamadas;
        public bool Expirar, NoEncontrado, Fallecido, RespuestaInvalida, FormatoReal;
        public bool NoAutorizado;
        public string? Cuerpo;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Interlocked.Increment(ref Llamadas);
            Check(request.Method == HttpMethod.Post, "Debe consultar por POST");
            if (RespuestaInvalida) return new(HttpStatusCode.OK) { Content = new StringContent("<html>Error</html>") };
            var url = request.RequestUri!.AbsoluteUri;
            if (url == "https://consultareceptores.feel.com.gt/rest/action")
            {
                if (Cuerpo is not null) return Json(Cuerpo);
                using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
                Check(json.RootElement.GetProperty("emisor_codigo").GetString() == "usuario-prueba" &&
                    json.RootElement.GetProperty("emisor_clave").GetString() == "llave-prueba" &&
                    json.RootElement.GetProperty("nit_consulta").GetString() == "1170142K", "Contrato JSON NIT");
                return Json(NoEncontrado ? "{\"resultado\":false,\"razon_social\":null}" : FormatoReal
                    ? "{\"nit\":\"1170142K\",\"nombre\":\"NEGOCIO DEMO\",\"mensaje\":\"\"}"
                    : "{\"resultado\":true,\"razon_social\":\"NEGOCIO DEMO\"}");
            }
            Check(request.Content is MultipartFormDataContent, "Login y CUI usan multipart/form-data");
            var form = (MultipartFormDataContent)request.Content!;
            if (url.EndsWith("/login"))
            {
                Check(form.Any(p => p.Headers.ContentDisposition!.Name!.Trim('"') == "prefijo") &&
                    form.Any(p => p.Headers.ContentDisposition!.Name!.Trim('"') == "llave"), "Credenciales form-data");
                var n = Interlocked.Increment(ref Logins);
                return Json($"{{\"resultado\":true,\"token\":\"token-prueba-{n}\"}}");
            }
            Check(url == "https://certificador.feel.com.gt/api/v2/servicios/externos/cui" &&
                request.Headers.Authorization?.Scheme == "Bearer", "URL y Bearer CUI");
            Check(await form.Single().ReadAsStringAsync(ct) == "2993930620101", "Documento CUI enviado");
            if (NoAutorizado) return new(HttpStatusCode.Unauthorized);
            if (Cuerpo is not null) return Json(Cuerpo);
            if (Expirar) { Expirar = false; return new(HttpStatusCode.Unauthorized); }
            if (FormatoReal) return Json("{\"resultado\":true,\"cui\":{\"cui\":\"2993930620101\",\"nombre\":\"PERSONA DEMO\",\"fallecido\":\"NO\"}}");
            return Json(Fallecido ? "{\"resultado\":true,\"nombre\":\"PERSONA DEMO\",\"estatus\":\"FALLECIDO\"}"
                : "{\"resultado\":true,\"nombre\":\"PERSONA DEMO\",\"estatus\":\"ACTIVO\"}");
        }
        private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body) };
    }
}
