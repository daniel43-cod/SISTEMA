using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Conversacion;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
await using var db = new ChatDbContext(new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options);
await db.Database.EnsureCreatedAsync();
for (var i = 1; i <= 4; i++)
    db.CuentaClientes.Add(new CuentaCliente { IdCuentaCliente = i, CorreoElectronico = $"cliente{i}@test.invalid",
        PasswordHash = "no-login-test", Estado = i != 4, FechaCreacion = DateTime.UtcNow });
db.Conversaciones.AddRange(
    new Conversacion { IdConversacion = 1, IdCuentaCliente = 1 },
    new Conversacion { IdConversacion = 2, IdCuentaCliente = 1 },
    new Conversacion { IdConversacion = 3, IdCuentaCliente = 2 },
    new Conversacion { IdConversacion = 4, IdCuentaCliente = 1 });
db.Mensajes.AddRange(
    new Mensaje { IdMensaje = 1, IdConversacion = 1, MensajeRecibido = "Primer chat" },
    new Mensaje { IdMensaje = 2, IdConversacion = 2, MensajeRecibido = new string('A', 150) },
    new Mensaje { IdMensaje = 3, IdConversacion = 3, MensajeRecibido = "Privado de otro cliente" },
    new Mensaje { IdMensaje = 4, IdConversacion = 1, MensajeRecibido = "Continuación" });
await db.SaveChangesAsync();
var service = new ConversacionListarService(db);
var count = 0;
void Check(bool ok, string label) { if (!ok) throw new Exception(label); count++; Console.WriteLine("OK: " + label); }
var chats = await service.ListarChats(1);
Check(chats.Count == 2 && chats.All(c => c.IdConversacion is 1 or 2), "Aislamiento por cuenta y exclusión de conversaciones vacías");
Check(chats[0].IdConversacion == 1 && chats[0].Contenido == "Primer chat", "Orden por último mensaje y título del primero");
Check(chats[1].Contenido == new string('A', 100) + "…", "Texto del título limitado");
Check((await service.ListarChats(1, 2, 1)).Single().IdConversacion == 2, "Paginación");
Check((await service.ListarChats(3)).Count == 0, "Cliente nuevo sin chats devuelve lista vacía");
foreach (var id in new[] { 0, 4, 999 })
{
    var denied = false;
    try { await service.ListarChats(id); } catch (UnauthorizedAccessException) { denied = true; }
    Check(denied, $"Rechaza cuenta inválida o inactiva {id}");
}
var invalidPage = false;
try { await service.ListarChats(1, 0); } catch (ValidationException) { invalidPage = true; }
Check(invalidPage, "Valida página");
Console.WriteLine($"{count} comprobaciones aprobadas.");
var messages = await service.ListarMensajes(1, 1);
Check(messages!.Select(m => m.IdMensaje).SequenceEqual(new[] { 1, 4 }), "Mensajes en orden cronológico");
Check(await service.ListarMensajes(2, 1) is null, "No permite leer mensajes de otro cliente");
Check(await service.ListarMensajes(1, 999) is null, "Conversación inexistente no disponible");
Check((await service.ListarMensajes(1, 1, 4))!.Single().IdMensaje == 1, "Cursor recupera mensajes anteriores");
Check((await service.ListarMensajes(1, 4))!.Count == 0, "Conversación propia vacía");
Console.WriteLine($"{count} comprobaciones aprobadas en total.");

sealed class ChatDbContext(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var keep = new[] { typeof(CuentaCliente), typeof(Conversacion), typeof(Mensaje) };
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            if (!keep.Contains(entity.ClrType)) builder.Ignore(entity.ClrType);
        builder.Entity<Conversacion>().Ignore(c => c.Usuario);
        builder.Entity<Mensaje>().Property(m => m.MensajeRecibido).HasColumnType("TEXT");
        builder.Entity<Mensaje>().Property(m => m.Respuesta).HasColumnType("TEXT");
    }
}
