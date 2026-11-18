using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Productos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
await using var db = new InventoryDb(options);
await db.Database.EnsureCreatedAsync();
db.categorias.Add(new Categoria { IdCategoria = 1, nombreCategoria = "Bebidas", Estado = true });
db.Marcas.Add(new Marca { IdMarca = 1, IdCategoria = 1, Nombre = "Marca", Estado = true });
db.productos.AddRange(
    new Productos { IdProducto = 1, Nombre = "A", IdMarca = 1, Stock = 5, StockMinimo = 10, PrecioPromedioCompra = 100 },
    new Productos { IdProducto = 2, Nombre = "B", IdMarca = 1, Stock = 0, StockMinimo = 3, PrecioPromedioCompra = 10 },
    new Productos { IdProducto = 3, Nombre = "C", IdMarca = 1, Stock = 10, StockMinimo = 10, PrecioPromedioCompra = 4 },
    new Productos { IdProducto = 4, Nombre = "D", IdMarca = 1, Stock = 2, StockMinimo = null, PrecioPromedioCompra = 3 });
await db.SaveChangesAsync();
var service = new ProductoInventarioService(db);
void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("OK " + name); }
var first = await service.Listar(new() { TamanoPagina = 1 });
var next = await service.Listar(new() { TamanoPagina = 1, Pagina = 2 });
Check(first.Total == 4 && first.TotalProductos == 4 && first.ProductosStockBajo == 1 && first.ProductosAgotados == 1,
    "Resumen completo antes de paginar, sin inventar mínimos para productos sin umbral");
Check(first.ValorInventarioTotal == 546 && next.ValorInventarioTotal == 546 && first.Items.Single().ValorInventario == 500,
    "Denominador de participación incluye todas las páginas");
Check(first.Items.Single().StockMinimo == 10 && first.Items.Single().Categoria == "Bebidas", "Expone categoría y mínimo reales");
var low = await service.Listar(new() { EstadoStock = "bajo" });
Check(low.Total == 1 && low.ValorInventarioTotal == 500 && low.Items.Single().IdProducto == 1 && low.TotalProductos == 4,
    "Stock bajo excluye agotados y productos al mínimo; filtra antes de sumar");
var empty = await service.Listar(new() { EstadoStock = "agotado" });
Check(empty.Total == 1 && empty.Items.Single().IdProducto == 2 && empty.ValorInventarioTotal == 0, "Agotados con existencia cero");
var filtered = await service.Listar(new() { Texto = "C", IdMarca = 1, IdCategoria = 1 });
Check(filtered.TotalProductos == 1 && filtered.ProductosStockBajo == 0 && filtered.ProductosAgotados == 0 && filtered.ValorInventarioTotal == 40,
    "Tarjetas y barras respetan búsqueda, categoría y marca");
try { await service.Listar(new() { EstadoStock = "inventado" }); throw new Exception("Filtro inválido aceptado"); }
catch (ValidationException) { Console.WriteLine("OK Rechaza estados inválidos"); }
var controller = new API_SISTEMA.Controllers.ProductosController(null!, null!, null!, null!,
    Microsoft.Extensions.Logging.Abstractions.NullLogger<API_SISTEMA.Controllers.ProductosController>.Instance);
foreach (var estado in new[] { "agotado", "bajo" })
{
    var response = await controller.ListarInventario(service, estadoStock: estado);
    var result = (response.Result as Microsoft.AspNetCore.Mvc.OkObjectResult)?.Value as API_SISTEMA.Dtos.Productos.ProductoInventarioPaginaDto;
    Check(result?.Total == 1 && result.Items.Single().IdProducto == (estado == "agotado" ? 2 : 1),
        "Controlador recibe y reenvía estadoStock=" + estado);
}
var allResponse = await controller.ListarInventario(service);
Check(((allResponse.Result as Microsoft.AspNetCore.Mvc.OkObjectResult)?.Value as API_SISTEMA.Dtos.Productos.ProductoInventarioPaginaDto)?.Total == 4,
    "Controlador sin estadoStock conserva todos los productos");
var invalidResponse = await controller.ListarInventario(service, estadoStock: "invalido");
Check(invalidResponse.Result is Microsoft.AspNetCore.Mvc.BadRequestObjectResult, "Controlador rechaza un estado desconocido");
Console.WriteLine("11 comprobaciones de inventario aprobadas; SQLite temporal.");
sealed class InventoryDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        var keep = new HashSet<Type> { typeof(Productos), typeof(Marca), typeof(Categoria), typeof(ProductoPresentacion), typeof(Presentacion) };
        foreach (var e in b.Model.GetEntityTypes().ToArray())
            if (!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
    }
}