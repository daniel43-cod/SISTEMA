using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.WhatsApp;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

void Check(bool condicion, string nombre) { if (!condicion) throw new Exception(nombre); Console.WriteLine("OK: " + nombre); }
HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
var config = Options.Create(new WhatsAppOptions { AccessToken = "ficticio", PhoneNumberId = "12345" });
await using var conexion = new SqliteConnection("Data Source=:memory:"); await conexion.OpenAsync();
var opciones = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(conexion).Options;
await using var db = new TestDb(opciones); await db.Database.EnsureCreatedAsync();
var venta = new Ventas { id_ventas = 5, origen = "PRUEBA", total = 400, saldo_pendiente = 400, subtotal = 400,
    cliente = new Cliente { nombre = "Cliente original", telefono = "55551234" },
    DetalleVentas = Enumerable.Range(1, 40).Select(i => new DetalleVenta {
        id_detalle_venta = i, cantidad = 1, precio = 10, Producto = new Productos { Nombre = "Producto original" },
        producto_presentacion = new ProductoPresentacion { Presentacion = new Presentacion { Descripcion = "Unidad" } }
    }).ToList() };
var empresa = new Empresa { id_empresa = 1, nombre_empresa = "Negocio original" };
var hashes = new List<string>();
var destinatario = "50255551234";
bool fallarSegunda = true;
int mensajes = 0;
var handler = new Handler(async (r, n) => {
    if (r.RequestUri!.AbsolutePath.EndsWith("/media"))
    {
        Check(await db.WhatsAppEnvioIntentos.AsNoTracking().AnyAsync(i => i.estado == "subiendo" && i.fin_utc == null), "intento persistido antes de subir");
        var archivo = ((MultipartFormDataContent)r.Content!).Single(c => c.Headers.ContentDisposition!.Name!.Trim('"') == "file");
        hashes.Add(Convert.ToHexString(SHA256.HashData(await archivo.ReadAsByteArrayAsync())));
        return Json($"{{\"id\":\"media-{n}\"}}");
    }
    Check(await db.WhatsAppEnvioIntentos.AsNoTracking().AnyAsync(i => i.estado == "enviando" && i.fin_utc == null), "estado durable previo al envío");
    using var doc = JsonDocument.Parse(await r.Content!.ReadAsStringAsync());
    Check(doc.RootElement.GetProperty("to").GetString() == destinatario, "usa teléfono actual del cliente");
    mensajes++;
    if (fallarSegunda && mensajes == 2) return Json("{\"error\":{\"code\":131000}}", HttpStatusCode.BadRequest);
    return Json($"{{\"messages\":[{{\"id\":\"wamid.{n}\"}}]}}");
});
var servicio = new WhatsAppHistorialService(db, new WhatsAppClient(new HttpClient(handler), config), config);
var primero = await servicio.Iniciar(venta, empresa, 1, default);
Check(primero.Resultado.Estado == "parcial" && primero.Resultado.TotalPaginas == 2, "registra fallo parcial y detiene la secuencia");
Check(primero.HuboFallo && primero.Mensaje.Contains("Intenta nuevamente más tarde"), "aviso de fallo para el usuario");
fallarSegunda = false;
var segundo = await servicio.Iniciar(venta, empresa, 1, default);
Check(segundo.IdEnvio != primero.IdEnvio && segundo.Resultado.Estado == "aceptado" && handler.Llamadas == 8,
    "volver a compartir envía las dos páginas completas, incluida la antes aceptada");
var tercero = await servicio.Iniciar(venta, empresa, 1, default);
Check(tercero.IdEnvio != segundo.IdEnvio && handler.Llamadas == 12, "permite compartir de nuevo aunque no cambie la venta");
Check(!tercero.HuboFallo && tercero.Mensaje.Contains("pendiente de confirmación"), "no promete entrega solo por aceptación");
Check((await servicio.Consultar(primero.IdEnvio, default)).Resultado.Estado == "parcial", "conserva el resultado de cada envío anterior");

var primerHash = hashes[0];
venta.total = 19; venta.subtotal = 19; venta.cliente.nombre = "Cliente actualizado"; venta.cliente.telefono = "44441234";
venta.DetalleVentas = [new() { cantidad = 1, precio = 19, Producto = new Productos { Nombre = "Producto modificado" },
    producto_presentacion = new ProductoPresentacion { Presentacion = new Presentacion { Descripcion = "Caja" } } }];
empresa.nombre_empresa = "Empresa actualizada"; destinatario = "50244441234";
var actualizado = await servicio.Iniciar(venta, empresa, 2, default);
Check(actualizado.Resultado.TotalPaginas == 1 && hashes[^1] != primerHash, "venta modificada genera una imagen nueva con datos actuales");
Check(await db.WhatsAppEnvios.CountAsync(e => e.id_venta == 5 && e.id_empresa == 1) == 4, "varios envíos de una misma venta quedan registrados");
var entidad = db.Model.FindEntityType(typeof(WhatsAppEnvio))!;
Check(entidad.FindProperty("snapshot_json") == null && entidad.FindProperty("bloqueo") == null,
    "el modelo ya no guarda copias JSON ni bloqueos de la venta");
Check(!entidad.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { "id_venta", "id_empresa" })).IsUnique,
    "índice de venta y empresa permite compartir repetidamente");

var rechazado = new Handler((r, n) => Task.FromResult(Json("{\"error\":{\"code\":190}}", HttpStatusCode.Unauthorized)));
var conError = new WhatsAppHistorialService(db, new WhatsAppClient(new HttpClient(rechazado), config), config);
var error = await conError.Iniciar(venta, empresa, 1, default);
Check(error.HuboFallo && error.Resultado.Paginas[0].CodigoMeta == 190 && rechazado.Llamadas == 1, "error de subida informa fallo sin enviar plantilla");
var nuevoIntento = await servicio.Iniciar(venta, empresa, 1, default);
Check(nuevoIntento.IdEnvio != error.IdEnvio && nuevoIntento.Resultado.Estado == "aceptado", "un fallo anterior no bloquea otro Compartir");

var timeout = new Handler((r, n) => n == 1 ? Task.FromResult(Json("{\"id\":\"media\"}")) : throw new TaskCanceledException());
var incierto = await new WhatsAppHistorialService(db, new WhatsAppClient(new HttpClient(timeout), config), config).Iniciar(venta, empresa, 1, default);
Check(incierto.HuboFallo && incierto.Resultado.Estado == "desconocido" && timeout.Llamadas == 2,
    "timeout informa fallo y no reintenta automáticamente");
var despuesTimeout = await servicio.Iniciar(venta, empresa, 1, default);
Check(despuesTimeout.IdEnvio != incierto.IdEnvio && despuesTimeout.Resultado.Estado == "aceptado", "Compartir explícito vuelve a enviar después de un resultado desconocido");

// Dos acciones explícitas pueden coincidir; cada una tiene su propio envío.
var interno = new Handler((r, n) => Task.FromResult(Json(r.RequestUri!.AbsolutePath.EndsWith("/media")
    ? "{\"id\":\"media-interno\"}" : "{\"messages\":[{\"id\":\"wamid.interno\"}]}")));
Guid? anidadoId = null;
var externo = new Handler(async (r, n) => {
    if (n == 1)
    {
        await using var otraDb = new TestDb(opciones);
        var paralelo = new WhatsAppHistorialService(otraDb, new WhatsAppClient(new HttpClient(interno), config), config);
        anidadoId = (await paralelo.Iniciar(venta, empresa, 2, default)).IdEnvio;
    }
    return Json(r.RequestUri!.AbsolutePath.EndsWith("/media") ? "{\"id\":\"media-externo\"}" : "{\"messages\":[{\"id\":\"wamid.externo\"}]}" );
});
var externoResultado = await new WhatsAppHistorialService(db, new WhatsAppClient(new HttpClient(externo), config), config).Iniciar(venta, empresa, 1, default);
Check(anidadoId.HasValue && anidadoId != externoResultado.IdEnvio && interno.Llamadas == 2 && externo.Llamadas == 2,
    "acciones simultáneas no se bloquean entre sí ni reutilizan el envío");
using var historial = JsonDocument.Parse(JsonSerializer.Serialize(await servicio.Historial(5, 1, default, 1, 2)));
Check(historial.RootElement.GetProperty("totalEnvios").GetInt32() == 10 && historial.RootElement.GetProperty("envios").GetArrayLength() == 2,
    "historial lista todos los envíos con paginación");
Check(!historial.RootElement.GetRawText().Contains("snapshot_json") && !historial.RootElement.GetRawText().Contains("ficticio"), "historial no expone copias ni secretos");
var sinTelefono = new Ventas { id_ventas = 30, origen = "PRUEBA", cliente = new Cliente { nombre = "Sin teléfono" },
    DetalleVentas = venta.DetalleVentas, total = 19 };
destinatario = "50233331234";
var temporal = await servicio.Iniciar(sinTelefono, empresa, 1, default, "3333-1234");
Check(temporal.Resultado.Estado == "aceptado" && sinTelefono.cliente.telefono == null,
    "teléfono temporal del historial no modifica venta ni cliente");
try { WhatsAppHistorialService.ResolverTelefono(venta, "33331234"); throw new Exception("Permitió cambiar teléfono registrado."); }
catch (System.ComponentModel.DataAnnotations.ValidationException) { Check(true, "teléfono existente no se sustituye por un temporal"); }

await using var contactoConexion = new SqliteConnection("Data Source=:memory:"); await contactoConexion.OpenAsync();
var contactoOpciones = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(contactoConexion).Options;
await using var contactoDb = new ContactoDb(contactoOpciones); await contactoDb.Database.EnsureCreatedAsync();
var clienteSin = new Cliente { id_cliente = 1, nombre = "Cliente sin teléfono" };
var ventaSin = new Ventas { id_ventas = 40, id_cliente = 1, cliente = clienteSin, origen = "PRUEBA" };
contactoDb.Add(clienteSin); contactoDb.Add(ventaSin); await contactoDb.SaveChangesAsync();
var contactoServicio = new WhatsAppHistorialService(contactoDb, new WhatsAppClient(new HttpClient(handler), config), config);
await contactoServicio.GuardarTelefonoRegistro(ventaSin, "5555-1234", default);
Check((await contactoDb.cliente.AsNoTracking().SingleAsync()).telefono == "50255551234", "teléfono ingresado al registrar queda guardado en cliente.telefono");
Check(contactoDb.Model.FindEntityType(typeof(Ventas))!.FindProperty("telefono_comprobante") == null, "venta ya no requiere otra columna de teléfono");
try { await contactoServicio.GuardarTelefonoRegistro(ventaSin, "44441234", default); throw new Exception("Sobrescribió teléfono."); }
catch (System.ComponentModel.DataAnnotations.ValidationException) { Check(true, "el teléfono del cliente ya guardado no puede sobrescribirse al enviar"); }
Console.WriteLine("Pruebas completadas: compartir completo, teléfonos por flujo, SQL temporal y Meta simulado.");

sealed class Handler(Func<HttpRequestMessage, int, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    public int Llamadas;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => responder(request, ++Llamadas);
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

sealed class ContactoDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        foreach (var entity in model.Model.GetEntityTypes().ToArray())
            if (entity.ClrType != typeof(Ventas) && entity.ClrType != typeof(Cliente)) model.Ignore(entity.ClrType);
        model.Entity<Ventas>().Ignore(v => v.DetalleVentas).Ignore(v => v.EstadoVenta).Ignore(v => v.usuario).Ignore(v => v.sesionCaja);
        model.Entity<Ventas>().ToTable("ventas").HasOne(v => v.cliente).WithMany().HasForeignKey(v => v.id_cliente);
        model.Entity<Cliente>().ToTable("cliente");
    }
}
