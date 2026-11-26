using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
await using var db = new CheckDb(options);
await db.Database.EnsureCreatedAsync();
db.rols.Add(new Rol { id_rol = 1, nombre = "ADMINISTRADOR", descripcion = "Prueba", estado = true });
db.usuarios.Add(new Usuario { id_usuario = 1, id_rol = 1, nombre = "Admin", apellido = "Prueba", usuario = "admin", telefono = "123", estado = true });
db.caja.Add(new Caja { id_caja = 1, nombre_caja = "Caja", estado = true });
db.sesioncaja.Add(new SesionCaja { id_sesion_caja = 1, id_caja = 1, id_usuario_apertura = 1, fecha_apertura = DateTime.Now });
db.estado_compras.AddRange(new EstadoCompra { id_estado_compra = 1, nombre_estado_compra = "Pagada", descripcion = "Pagada" },
    new EstadoCompra { id_estado_compra = 2, nombre_estado_compra = "Pendiente", descripcion = "Pendiente" });
db.categorias.Add(new Categoria { IdCategoria = 1, nombreCategoria = "Prueba", Estado = true });
db.Marcas.Add(new Marca { IdMarca = 1, Nombre = "Marca", IdCategoria = 1, Estado = true });
db.productos.Add(new Productos { IdProducto = 1, IdMarca = 1, Nombre = "Producto", Stock = 0, PrecioPromedioCompra = 0 });
await db.SaveChangesAsync();
var service = new CrearCompraService(db, new MovimientoCajaService(db));
int passed = 0;
void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("OK " + message); passed++; }
RegistroComprasDto Request(string? name = null, int? id = null) => new() { id_operacion = Guid.NewGuid(),
    nombre_proveedor = name, id_proveedor = id,
    detalle_compra = [new() { id_producto = 1, cantidad = 2, precio = 10 }]
};
async Task Reject(RegistroComprasDto request, string message) {
    try { await service.CrearCompra(request, 1); throw new Exception("Se aceptó: " + message); }
    catch (CompraValidationException) { Check(true, message); }
}
var first = await service.CrearCompra(Request("  Distribuidora Nueva  "), 1);
Check(await db.proveedores.CountAsync() == 1 && (await db.proveedores.SingleAsync()).nombre == "Distribuidora Nueva", "Crear distribuidora al guardar compra y recortar espacios");
Check(first.IdProveedor > 0 && first.TotalCompra == 10 && first.SaldoPendiente == 10 && (await db.productos.SingleAsync()).Stock == 2, "Compra, proveedor e inventario se guardan juntos");
var selected = await service.CrearCompra(Request(id: first.IdProveedor), 1);
Check(selected.IdProveedor == first.IdProveedor && await db.proveedores.CountAsync() == 1, "Seleccionar ID no crea ni modifica distribuidoras");
var reused = await service.CrearCompra(Request(" distribuidora NUEVA "), 1);
Check(reused.IdProveedor == first.IdProveedor && await db.proveedores.CountAsync() == 1, "Nombre existente ignora mayúsculas y espacios exteriores");
await Reject(Request(), "Rechazar proveedor ausente");
await Reject(Request("   "), "Rechazar nombre vacío");
await Reject(Request("Otra", first.IdProveedor), "Rechazar ID y nombre simultáneos");
await Reject(Request(id: 0), "Rechazar ID cero");
await Reject(Request(id: -1), "Rechazar ID negativo");
await Reject(Request(id: 99999), "Rechazar ID inexistente sin crear proveedor");
await Reject(Request(new string('x', 101)), "Rechazar nombre demasiado largo");
await Reject(Request("Nombre\nOculto"), "Rechazar controles en nombre");
var invalidProduct = Request("No guardar producto"); invalidProduct.detalle_compra[0].id_producto = 99999;
await Reject(invalidProduct, "Producto inválido no crea distribuidora");
var invalidPayment = Request("No guardar pago"); invalidPayment.monto_pagado = 11;
await Reject(invalidPayment, "Pago inválido no crea distribuidora");
Check(await db.proveedores.CountAsync() == 1 && await db.registroCompras.CountAsync() == 3, "Rechazos no dejan registros parciales");
var accented = await service.CrearCompra(Request("Distribuidora Simultánea"), 1);
var accentedAgain = await service.CrearCompra(Request("Distribuidora Simultánea"), 1);
Check(accented.IdProveedor == accentedAgain.IdProveedor && await db.proveedores.CountAsync() == 2, "Repetir nombre con tilde no crea otra distribuidora");
// La inserción del proveedor ocurre antes de guardar la compra. Una FK inválida debe revertir ambos.
try { await service.CrearCompra(Request("Revertir distribuidora"), 99999); throw new Exception("Se aceptó usuario inexistente"); }
catch (DbUpdateException) { db.ChangeTracker.Clear(); }
Check(!await db.proveedores.AnyAsync(p => p.nombre == "Revertir distribuidora") && await db.registroCompras.CountAsync() == 5, "Error al persistir compra revierte la distribuidora recién insertada");
await db.sesioncaja.ExecuteUpdateAsync(s => s.SetProperty(c => c.fecha_cierre, (DateTime?)DateTime.Now));
try { await service.CrearCompra(Request("Caja cerrada"), 1); throw new Exception("Caja cerrada aceptada"); }
catch (CajaValidationException) { Check(!await db.proveedores.AnyAsync(p => p.nombre == "Caja cerrada"), "Caja cerrada no crea distribuidora"); }
Console.WriteLine($"{passed} comprobaciones de proveedor en compras aprobadas (SQLite temporal).");

sealed class CheckDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options) {
    protected override void OnModelCreating(ModelBuilder b) {
        base.OnModelCreating(b);
        var keep = new HashSet<Type> { typeof(Usuario), typeof(Rol), typeof(Caja), typeof(SesionCaja), typeof(Proveedores),
            typeof(RegistroCompras), typeof(EstadoCompra), typeof(Productos), typeof(Marca), typeof(Categoria), typeof(DetalleCompra) };
        foreach (var e in b.Model.GetEntityTypes().ToArray()) if (!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
    }
}
