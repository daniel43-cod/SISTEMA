using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using API_SISTEMA.Models;
using API_SISTEMA.Services.WhatsApp;
using Microsoft.Extensions.Options;

static void Verificar(bool condicion, string mensaje) { if (!condicion) throw new Exception(mensaje); }
static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
static WhatsAppOptions Config() => new() { AccessToken = "token-ficticio-no-real", PhoneNumberId = "123456", BusinessAccountId = "98765" };
static WhatsAppClient Crear(Handler handler, WhatsAppOptions? config = null) => new(new HttpClient(handler), Options.Create(config ?? Config()));
static Ventas Venta(int detalles = 2, string? telefono = "5555-1234") => new() {
    id_ventas = 10, origen = "MOSTRADOR", cliente = new Cliente { nombre = "Cliente prueba", telefono = telefono },
    subtotal = detalles * 10, total = detalles * 10, saldo_pendiente = detalles * 10,
    DetalleVentas = Enumerable.Range(1, detalles).Select(i => new DetalleVenta {
        id_detalle_venta = i, cantidad = 1, precio = 10, subtotal = 10,
        Producto = new Productos { Nombre = "Producto de prueba" },
        producto_presentacion = new ProductoPresentacion { Presentacion = new Presentacion { Descripcion = "Unidad" } }
    }).ToList()
};
var empresa = new Empresa { nombre_empresa = "Empresa de prueba" };

foreach (var (entrada, esperado) in new[] { ("5555-1234", "50255551234"), ("+502 (5555) 1234", "50255551234"), ("+1 202 555 0123", "12025550123") })
    Verificar(EnviarComprobanteWhatsAppService.NormalizarTelefono(entrada) == esperado, "Normalización incorrecta.");
foreach (var entrada in new string?[] { null, "", "123", "55551234/55551235", "abcdefgh", "0000000000000000" })
{
    try { EnviarComprobanteWhatsAppService.NormalizarTelefono(entrada); throw new Exception("Aceptó teléfono inválido."); }
    catch (ValidationException) { }
}
Console.WriteLine("OK: teléfonos locales, internacionales e inválidos.");

var correcto = new Handler(async (r, n) => {
    Verificar(r.RequestUri!.Host == "graph.facebook.com", "Host incorrecto.");
    Verificar(r.Headers.Authorization?.Parameter == "token-ficticio-no-real", "Falta autorización.");
    if (n % 2 == 1)
    {
        Verificar(r.RequestUri.AbsolutePath.EndsWith("/123456/media"), "No subió antes de enviar.");
        var multipart = (MultipartFormDataContent)r.Content!;
        var archivo = multipart.Single(c => c.Headers.ContentDisposition!.Name!.Trim('"') == "file");
        Verificar(archivo.Headers.ContentType!.MediaType == "image/png", "No es PNG.");
        var bytes = await archivo.ReadAsByteArrayAsync();
        Verificar(bytes.Length > 8 && bytes[0] == 137 && bytes[1] == 80, "PNG inválido.");
        return Json($"{{\"id\":\"media-{n}\"}}");
    }
    Verificar(r.RequestUri.AbsolutePath.EndsWith("/123456/messages"), "Ruta incorrecta.");
    using var doc = JsonDocument.Parse(await r.Content!.ReadAsStringAsync());
    var root = doc.RootElement;
    Verificar(root.GetProperty("to").GetString() == "50255551234", "No usó el teléfono del cliente.");
    Verificar(root.GetProperty("type").GetString() == "template", "No usa plantilla.");
    var template = root.GetProperty("template");
    Verificar(template.GetProperty("name").GetString() == "envio_de_comprobante", "Nombre incorrecto.");
    Verificar(template.GetProperty("language").GetProperty("code").GetString() == "es_PA", "Idioma incorrecto.");
    var components = template.GetProperty("components");
    Verificar(components.GetArrayLength() == 1 && components[0].GetProperty("type").GetString() == "header", "Envía componentes no solicitados.");
    Verificar(components[0].GetProperty("parameters")[0].GetProperty("image").GetProperty("id").GetString() == $"media-{n - 1}", "Desorden de imágenes.");
    return Json($"{{\"messages\":[{{\"id\":\"wamid.{n}\"}}]}}");
});
var servicio = new EnviarComprobanteWhatsAppService(Crear(correcto));
var una = await servicio.Enviar(Venta(), empresa, default);
Verificar(una.Estado == "aceptado" && una.TotalPaginas == 1 && correcto.Llamadas == 2, "Falló una imagen.");
correcto.Llamadas = 0;
var muchas = await servicio.Enviar(Venta(40), empresa, default);
Verificar(muchas.TotalPaginas > 1 && muchas.Estado == "aceptado", "Falló multipágina.");
Verificar(muchas.Paginas.Select(p => p.Pagina).SequenceEqual(Enumerable.Range(1, muchas.TotalPaginas)), "Páginas desordenadas.");
Verificar(correcto.Llamadas == muchas.TotalPaginas * 2, "Solicitudes adicionales.");
Console.WriteLine($"OK: carga PNG, plantilla y envío secuencial de {muchas.TotalPaginas} páginas.");

var fallo = new Handler((r, n) => Task.FromResult(n switch {
    1 => Json("{\"id\":\"media-1\"}"), 2 => Json("{\"messages\":[{\"id\":\"wamid.primero\"}]}"),
    3 => Json("{\"id\":\"media-2\"}"),
    _ => Json("{\"error\":{\"code\":190,\"message\":\"token-ficticio-no-real\"}}", HttpStatusCode.Unauthorized)
}));
var parcial = await new EnviarComprobanteWhatsAppService(Crear(fallo)).Enviar(Venta(70), empresa, default);
Verificar(parcial.Estado == "parcial" && fallo.Llamadas == 4, "No detuvo después del rechazo.");
Verificar(parcial.Paginas[0].MessageId == "wamid.primero" && parcial.Paginas[1].CodigoMeta == 190, "Perdió estado previo.");
Verificar(parcial.Paginas.Skip(2).All(p => p.Estado == "no_intentado"), "Intentó páginas posteriores.");
Verificar(!JsonSerializer.Serialize(parcial).Contains("token-ficticio"), "Exposición de credenciales.");
Console.WriteLine("OK: rechazo parcial, sin reintentos ni exposición del error privado.");

foreach (var tipo in new[] { "timeout", "red", "json", "sin-id", "servidor" })
{
    var handler = new Handler((r, n) => {
        if (n == 1) return Task.FromResult(Json("{\"id\":\"media\"}"));
        if (tipo == "timeout") throw new TaskCanceledException();
        if (tipo == "red") throw new HttpRequestException("dato privado");
        return Task.FromResult(tipo == "json" ? Json("no es json") : tipo == "sin-id" ? Json("{}") : Json("{}", HttpStatusCode.InternalServerError));
    });
    var respuesta = await new EnviarComprobanteWhatsAppService(Crear(handler)).Enviar(Venta(40), empresa, default);
    Verificar(respuesta.Estado == "desconocido" && handler.Llamadas == 2, "No trató envío incierto sin reintentar.");
}
Console.WriteLine("OK: timeout, transporte, JSON inválido, respuesta sin ID y error 500.");

var noRed = new Handler((r, n) => throw new Exception("No debe hacer llamadas HTTP."));
foreach (var falta in new[] { "telefono", "configuracion" })
{
    try
    {
        var config = Config(); if (falta == "configuracion") config.AccessToken = "";
        await new EnviarComprobanteWhatsAppService(Crear(noRed, config)).Enviar(Venta(2, falta == "telefono" ? null : "55551234"), empresa, default);
        throw new Exception("Debió validar antes de enviar.");
    }
    catch (ValidationException) { }
}
try { await Crear(noRed).SubirImagen(new byte[5 * 1024 * 1024 + 1], default); throw new Exception("Aceptó imagen excesiva."); }
catch (ValidationException) { }
using var cancelado = new CancellationTokenSource(); cancelado.Cancel();
var cancelacion = await new EnviarComprobanteWhatsAppService(Crear(noRed)).Enviar(Venta(), empresa, cancelado.Token);
Verificar(cancelacion.Paginas.All(p => p.Estado == "no_intentado") && noRed.Llamadas == 0, "Ignoró cancelación.");
Console.WriteLine("OK: configuración, teléfono, tamaño y cancelación previos a la red.");

var subidaFalla = new Handler((r, n) => Task.FromResult(Json("{\"error\":{\"code\":131053}}", HttpStatusCode.BadRequest)));
var sinEnvio = await new EnviarComprobanteWhatsAppService(Crear(subidaFalla)).Enviar(Venta(), empresa, default);
Verificar(subidaFalla.Llamadas == 1 && sinEnvio.Paginas[0].Etapa == "subida", "Intentó enviar sin media ID.");
Console.WriteLine("OK: fallo de subida impide el envío de plantilla.");

sealed class Handler(Func<HttpRequestMessage, int, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    public int Llamadas;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => responder(request, ++Llamadas);
}
