using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.CatalogoPublico;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
await using var db = new PublicDbContext(options);
await db.Database.EnsureCreatedAsync();
db.categorias.Add(new Categoria { IdCategoria = 1, nombreCategoria = "Bebidas", Estado = true });
db.Marcas.AddRange(
    new Marca { IdMarca = 1, Nombre = "Marca activa", IdCategoria = 1, Estado = true },
    new Marca { IdMarca = 2, Nombre = "Marca inactiva", IdCategoria = 1, Estado = false },
    new Marca { IdMarca = 3, Nombre = "Marca vacía", IdCategoria = 1, Estado = true });
db.presentaciones.AddRange(
    new Presentacion { IdPresentacion = 1, Descripcion = "Unidad", Estado = true },
    new Presentacion { IdPresentacion = 2, Descripcion = "Caja", Estado = true },
    new Presentacion { IdPresentacion = 3, Descripcion = "Oculta", Estado = false });
db.productos.AddRange(
    new Productos { id_producto = 1, nombre = "Agua", IdMarca = 1, stock = 5 },
    new Productos { id_producto = 2, nombre = "Jugo", IdMarca = 1, stock = 0 },
    new Productos { id_producto = 3, nombre = "Sin presentaciones públicas", IdMarca = 1, stock = 20 });
db.producto_presentaciones.AddRange(
    new ProductoPresentacion { id_producto_presentacion = 1, id_producto = 1, IdPresentacion = 1, estado = true, unidades_equivalentes = 1, precio = 5 },
    new ProductoPresentacion { id_producto_presentacion = 2, id_producto = 1, IdPresentacion = 2, estado = true, unidades_equivalentes = 12, precio = 50 },
    new ProductoPresentacion { id_producto_presentacion = 3, id_producto = 1, IdPresentacion = 3, estado = true, unidades_equivalentes = 1, precio = 2 },
    new ProductoPresentacion { id_producto_presentacion = 4, id_producto = 2, IdPresentacion = 1, estado = true, unidades_equivalentes = 1, precio = 6 },
    new ProductoPresentacion { id_producto_presentacion = 5, id_producto = 3, IdPresentacion = 1, estado = false, unidades_equivalentes = 1, precio = 6 });
await db.SaveChangesAsync();
var service = new CatalogoPublicoService(db);
var passed = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("OK: " + message);
    passed++;
}
var first = await service.ListarProductosPorMarca(1, 1, 1);
Check(first is { HayMas: true } && first.Productos.Single().Nombre == "Agua", "Paginación ordenada y siguiente página");
var presentations = first!.Productos.Single().Presentaciones;
Check(presentations.Count == 2 && presentations[0].Disponible && !presentations[1].Disponible,
    "Presentaciones activas y disponibilidad por equivalencia");
Check(presentations[0].Precio == 5 && presentations[1].Precio == 50, "Precios de venta por presentación");
var second = await service.ListarProductosPorMarca(1, 2, 1);
Check(second is { HayMas: false } && second.Productos.Single().Nombre == "Jugo" &&
    !second.Productos.Single().Presentaciones.Single().Disponible, "Agotados visibles y presentaciones inactivas excluidas");
Check((await service.ListarProductosPorMarca(3))!.Productos.Count == 0, "Marca activa sin productos devuelve lista vacía");
Check(await service.ListarProductosPorMarca(2) is null && await service.ListarProductosPorMarca(999) is null,
    "Marcas inactivas o inexistentes no disponibles");
(await db.categorias.SingleAsync()).Estado = false;
await db.SaveChangesAsync();
Check(await service.ListarProductosPorMarca(1) is null, "Categoría inactiva bloquea productos de sus marcas");
try { await service.ListarProductosPorMarca(1, 0); throw new Exception("Aceptó página inválida"); }
catch (ArgumentOutOfRangeException) { Check(true, "Página inválida rechazada"); }
Console.WriteLine($"{passed} comprobaciones aprobadas, con base temporal.");

sealed class PublicDbContext(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var keep = new[] { typeof(Productos), typeof(Marca), typeof(Categoria), typeof(ProductoPresentacion), typeof(Presentacion) };
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            if (!keep.Contains(entity.ClrType)) builder.Ignore(entity.ClrType);
        builder.Entity<Productos>().Ignore(p => p.DetalleVentas).Ignore(p => p.ProductoPrecios);
    }
}
