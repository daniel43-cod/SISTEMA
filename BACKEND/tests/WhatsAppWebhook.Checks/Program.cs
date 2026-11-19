using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API_SISTEMA.Controllers;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.WhatsApp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

void Check(bool condition, string nombre) { if (!condition) throw new Exception(nombre); Console.WriteLine("OK: " + nombre); }
var config = Options.Create(new WhatsAppOptions { AccessToken = "ficticio", PhoneNumberId = "12345", BusinessAccountId = "67890",
    AppSecret = "secreto-solo-pruebas", VerifyToken = "verificacion-solo-pruebas" });
await using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
await using var db = new TestDb(options); await db.Database.EnsureCreatedAsync();
var servicio = new WhatsAppWebhookService(db, config);
var controller = new WhatsAppWebhookController(servicio, config);
Check(controller.Verificar("subscribe", config.Value.VerifyToken, "98765") is ContentResult { Content: "98765", ContentType: "text/plain" }, "challenge exacto en texto plano");
Check(controller.Verificar("subscribe", "incorrecto", "98765") is StatusCodeResult { StatusCode: 403 }, "verificación rechaza token distinto");
Check(controller.Verificar("otro", config.Value.VerifyToken, "98765") is StatusCodeResult { StatusCode: 403 }, "verificación rechaza modo incorrecto");

byte[] Payload(string id, string estado, string timestamp = "1700000000", string cuenta = "67890", string remitente = "12345") => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
    @object = "whatsapp_business_account", entry = new[] { new { id = cuenta, changes = new[] { new { field = "messages", value = new {
        metadata = new { phone_number_id = remitente }, statuses = new[] { new { id, status = estado, timestamp, errors = new[] { new { code = 131026 } } } }
    } } } } }
}));
string Firma(byte[] body) => "sha256=" + Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(config.Value.AppSecret), body)).ToLowerInvariant();
var body = Payload("wamid.1", "delivered");
Check(WhatsAppWebhookService.ValidarFirma(body, Firma(body), config.Value.AppSecret), "HMAC válido sobre bytes originales");
Check(!WhatsAppWebhookService.ValidarFirma(body.Concat(new byte[] { 32 }).ToArray(), Firma(body), config.Value.AppSecret), "alteración del cuerpo invalida firma");
Check(!WhatsAppWebhookService.ValidarFirma(body, "sha256=" + new string('z', 64), config.Value.AppSecret), "firma malformada rechazada");
Check(!WhatsAppWebhookService.ValidarFirma(body, null, config.Value.AppSecret), "firma ausente rechazada");
controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
controller.Request.Body = new MemoryStream(body);
controller.Request.Headers["X-Hub-Signature-256"] = "sha256=" + new string('0', 64);
Check(await controller.Recibir(default) is StatusCodeResult { StatusCode: 401 } && await db.WhatsAppWebhookEventos.CountAsync() == 0,
    "POST no autenticado no escribe datos");
controller.Request.Body = new MemoryStream(body); controller.Request.Headers["X-Hub-Signature-256"] = Firma(body);
Check(await controller.Recibir(default) is OkResult, "POST firmado aceptado tras persistir");
await servicio.Guardar(body, default);
Check(await db.WhatsAppWebhookEventos.CountAsync() == 1, "notificación repetida deduplicada");
await servicio.Guardar(Payload("wamid.ajeno", "read", cuenta: "otra"), default);
await servicio.Guardar(Payload("wamid.ajeno", "read", remitente: "otro"), default);
Check(await db.WhatsAppWebhookEventos.CountAsync() == 1, "cuenta o remitente ajenos ignorados");
await servicio.Guardar(Encoding.UTF8.GetBytes("{\"object\":\"whatsapp_business_account\",\"entry\":[]}"), default);
await servicio.Guardar(Payload("wamid.1", "sent", "1699999999"), default);
await servicio.Guardar(Payload("wamid.1", "read", "1700000001"), default);
await servicio.Guardar(Payload("wamid.1", "failed", "1700000002"), default);
var eventos = await db.WhatsAppWebhookEventos.AsNoTracking().ToListAsync();
Check(WhatsAppEntrega.Ultimo(eventos)?.estado == "read", "eventos tardíos no hacen retroceder lectura");

var venta = new Ventas { id_ventas = 1, origen = "PRUEBA", cliente = new Cliente { nombre = "Prueba", telefono = "55551234" }, total = 10 };
var empresa = new Empresa { id_empresa = 1, nombre_empresa = "Negocio prueba" };
var envio = new WhatsAppEnvio { id_envio = Guid.NewGuid(), id_venta = 1, id_empresa = 1, id_usuario = 1,
    telefono = "50255551234", plantilla = config.Value.TemplateName, idioma = config.Value.TemplateLanguage, remitente = config.Value.PhoneNumberId,
    creado_utc = DateTime.UtcNow,
    paginas = [new() { pagina = 1, estado = "aceptado", message_id = "wamid.1" }] };
db.Add(envio); await db.SaveChangesAsync();
var handler = new Handler();
var historial = new WhatsAppHistorialService(db, new WhatsAppClient(new HttpClient(handler), config), config);
var resumen = await historial.Consultar(envio.id_envio, default);
Check(resumen.Resultado.Estado == "leido" && resumen.Resultado.Paginas[0].EstadoEntrega == "leido", "correlación de evento recibido antes de guardar message ID");
var compartidoOtraVez = await historial.Iniciar(venta, empresa, 1, default);
Check(handler.Llamadas == 2 && compartidoOtraVez.IdEnvio != envio.id_envio, "lectura confirmada no bloquea compartir otra vez");

envio.paginas[0].message_id = "wamid.fallo"; await db.SaveChangesAsync();
await servicio.Guardar(Payload("wamid.fallo", "failed"), default);
var fallido = await historial.Consultar(envio.id_envio, default);
Check(fallido.Resultado.Estado == "fallido_entrega" && fallido.HuboFallo && fallido.Mensaje.Contains("más tarde"), "fallo de entrega informa al usuario");
var recuperado = await historial.Iniciar(venta, empresa, 2, default);
Check(handler.Llamadas == 4 && recuperado.IdEnvio != envio.id_envio, "Compartir crea envío completo nuevo tras fallo de entrega");
await servicio.Guardar(Payload("wamid.fallo", "failed", "1700000005"), default);
var nuevo = await historial.Consultar(recuperado.IdEnvio, default);
Check(nuevo.Resultado.Paginas[0].EstadoEntrega == null && nuevo.Resultado.Estado == "aceptado", "evento del intento viejo no altera el nuevo");
Check(await db.WhatsAppEnvioIntentos.CountAsync() == 2, "historial conserva ambos envíos explícitos");
Console.WriteLine("Webhook verificado con SQLite temporal y Meta simulado; sin mensajes reales.");

sealed class Handler : HttpMessageHandler
{
    public int Llamadas;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Llamadas++;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(
            request.RequestUri!.AbsolutePath.EndsWith("/media") ? "{\"id\":\"media\"}" : $"{{\"messages\":[{{\"id\":\"wamid.nuevo.{Llamadas}\"}}]}}" ) });
    }
}
sealed class TestDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        WhatsAppEnvioConfiguration.Configurar(model);
        var keep = new[] { typeof(WhatsAppEnvio), typeof(WhatsAppEnvioPagina), typeof(WhatsAppEnvioIntento), typeof(WhatsAppWebhookEvento) };
        foreach (var entity in model.Model.GetEntityTypes().ToArray()) if (!keep.Contains(entity.ClrType)) model.Ignore(entity.ClrType);
    }
}
