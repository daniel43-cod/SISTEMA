using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services;
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
db.sesioncaja.AddRange(new SesionCaja { id_sesion_caja = 1, id_caja = 1, id_usuario_apertura = 1, fecha_apertura = DateTime.Now, monto_inicial = 0 },
 new SesionCaja { id_sesion_caja = 2, id_caja = 1, id_usuario_apertura = 1, fecha_apertura = DateTime.Now.AddDays(-2), fecha_cierre = DateTime.Now.AddDays(-1), monto_inicial = 0 });
db.proveedores.AddRange(new Proveedores { id_proveedor = 1, nombre = "Proveedor A", telefono = "123", fecha_creacion = DateTime.Now },
 new Proveedores { id_proveedor = 2, nombre = "Proveedor B", telefono = "123", fecha_creacion = DateTime.Now });
db.estado_compras.Add(new EstadoCompra { id_estado_compra = 1, nombre_estado_compra = "Prueba", descripcion = "Prueba" });
db.registroCompras.AddRange(
 new RegistroCompras { IdCompra = 1, IdProveedor = 1, IdUsuario = 1, IdSesionCaja = 1, IdEstadoCompra = 1, FechaIngreso = DateTime.Now, TotalCompra = 300, SaldoPendiente = 200 },
 new RegistroCompras { IdCompra = 2, IdProveedor = 2, IdUsuario = 1, IdSesionCaja = 1, IdEstadoCompra = 1, FechaIngreso = DateTime.Now, TotalCompra = 100, SaldoPendiente = 0 },
 new RegistroCompras { IdCompra = 3, IdProveedor = 1, IdUsuario = 1, IdSesionCaja = 2, IdEstadoCompra = 1, FechaIngreso = DateTime.Now, TotalCompra = 900, SaldoPendiente = 900 });
await db.SaveChangesAsync();
var service = new CompraService(db, null!);
void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("OK " + name); }
var first = await service.Resumen(new() { TamanoPagina = 1 });
var second = await service.Resumen(new() { TamanoPagina = 1, Pagina = 2 });
Check(first.TotalComprado == 400 && first.TotalPorPagar == 200 && first.TotalPagado == 200 && first.TotalRegistros == 2,
 "Importes de la caja activa excluyen compras históricas");
Check(first.HayMas && !second.HayMas && first.TotalComprado == second.TotalComprado && first.Items.Single().id_compra != second.Items.Single().id_compra,
 "Totales independientes de la página");
var pending = await service.Resumen(new() { EstadoPago = "pendientes" });
Check(pending.TotalRegistros == 1 && pending.Items.Single().id_compra == 1 && pending.TotalComprado == 300 && pending.TotalPorPagar == 200,
 "Con saldo aplica antes de paginar y sumar");
var paid = await service.Resumen(new() { EstadoPago = "pagadas" });
Check(paid.TotalRegistros == 1 && paid.TotalPagado == 100 && paid.CantidadPendientes == 1 && paid.CantidadPagadas == 1, "Pagadas y conteos de pestañas");
Check((await service.Resumen(new() { Buscar = "Proveedor A" })).TotalComprado == 300 &&
 (await service.Resumen(new() { Buscar = "C-0002" })).Items.Single().id_compra == 2, "Búsqueda por proveedor o identificador");
await db.sesioncaja.Where(c => c.id_sesion_caja == 1).ExecuteUpdateAsync(s => s.SetProperty(c => c.fecha_cierre, (DateTime?)DateTime.Now));
Check((await service.Resumen(new())).TotalRegistros == 0, "Sin caja activa retorna resumen vacío");
Console.WriteLine("6 comprobaciones de compras aprobadas.");
sealed class CheckDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
 protected override void OnModelCreating(ModelBuilder b)
 {
  base.OnModelCreating(b);
  var keep = new HashSet<Type> { typeof(Usuario), typeof(Rol), typeof(Caja), typeof(SesionCaja), typeof(Proveedores), typeof(RegistroCompras), typeof(EstadoCompra) };
  foreach(var e in b.Model.GetEntityTypes().ToArray()) if(!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
 }
}