using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API_SISTEMA.Data;
using API_SISTEMA.DTOs.Surtidos;
using API_SISTEMA.Models;
using API_SISTEMA.Security;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Services.Surtidos;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

try { await Run(); }
catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }

static async Task Run()
{
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();
    connection.CreateFunction<string, string>("btrim", s => s.Trim());
    await using var db = new CheckDb(new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options);
    await db.Database.EnsureCreatedAsync();
    db.rols.Add(new Rol { id_rol = 1, nombre = "ADMINISTRADOR", descripcion = "Prueba", estado = true });
    db.usuarios.Add(new Usuario { id_usuario = 1, id_rol = 1, nombre = "Admin", apellido = "Prueba", usuario = "admin", telefono = "123", estado = true });
    db.categorias.Add(new Categoria { IdCategoria = 1, nombreCategoria = "Bebidas", Estado = true });
    db.Marcas.AddRange(new Marca { IdMarca = 1, IdCategoria = 1, Nombre = "Primera", Estado = true },
        new Marca { IdMarca = 2, IdCategoria = 1, Nombre = "Segunda", Estado = true });
    db.productos.AddRange(new Productos { IdProducto = 1, IdMarca = 1, Nombre = "Limón", PrecioPromedioCompra = 2.33m, Stock = 20 },
        new Productos { IdProducto = 2, IdMarca = 2, Nombre = "Uva", PrecioPromedioCompra = 2.70m, Stock = 10 });
    db.presentaciones.AddRange(new Presentacion { IdPresentacion = 1, Descripcion = "Bandeja", Estado = true },
        new Presentacion { IdPresentacion = 2, Descripcion = "Caja", Estado = true });
    await db.SaveChangesAsync();
    var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
    accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "test"));
    var service = new SurtidoService(db, new CatalogoAuditoriaService(db, new ContextoPeticion(accessor)));
    int passed = 0;
    void Check(bool ok, string message) { if (!ok) throw new Exception(message); passed++; Console.WriteLine("OK " + message); }
    CrearSurtidoDto Request() => new() { Nombre = "  Cola surtida  ", IdMarca = 1, IdsProductos = [1, 2],
        Presentaciones = [new() { IdPresentacion = 1, UnidadesEquivalentes = 6, Precio = 16.20m },
            new() { IdPresentacion = 2, UnidadesEquivalentes = 12, Precio = 32.40m }] };
    async Task Reject(CrearSurtidoDto dto, string message)
    {
        var count = await db.Surtidos.CountAsync();
        try { await service.Crear(dto, default); throw new Exception("Se aceptó " + message); }
        catch (ValidationException) { db.ChangeTracker.Clear(); Check(await db.Surtidos.CountAsync() == count, message); }
    }
    var created = await service.Crear(Request(), default);
    Check(created.Nombre == "Cola surtida" && created.IdMarca == 1 && created.Productos.Count == 2, "Crea surtido con productos de distintas marcas");
    Check(created.Presentaciones.Count == 2 && created.Presentaciones[0].PrecioMinimoActual == 16.20m, "Precio mínimo cubre todas las unidades del producto más caro");
    Check(await db.Set<ProductoPresentacion>().AllAsync(p => p.id_producto == null && p.id_surtido == created.IdSurtido), "Presentaciones pertenecen al surtido exclusivamente");
    Check(await db.productos.SumAsync(p => p.Stock) == 30, "Configurar no modifica inventario");
    Check(await db.AuditoriaEventos.CountAsync() == 1, "Configuración registra auditoría");
    Check((await service.Listar("COLA", 1, 20, default)).Total == 1, "Listado permite encontrar surtidos creados");
    var permission = (Microsoft.AspNetCore.Authorization.AuthorizeAttribute)Attribute.GetCustomAttribute(
        typeof(API_SISTEMA.Controllers.SurtidosController), typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))!;
    Check(permission.Roles == API_SISTEMA.Utilidades.Roles.Administrador, "Crear y consultar configuración exige rol administrador");
    var bad = Request(); bad.Presentaciones[0].Precio = 16.19m; await Reject(bad, "Rechaza precio bajo el máximo costo");
    bad = Request(); bad.Nombre = "   "; await Reject(bad, "Rechaza nombre vacío");
    bad = Request(); bad.IdsProductos = []; await Reject(bad, "Rechaza surtido sin productos");
    bad = Request(); bad.Presentaciones = []; await Reject(bad, "Rechaza surtido sin presentaciones");
    await db.Marcas.Where(m => m.IdMarca == 1).ExecuteUpdateAsync(s => s.SetProperty(m => m.Estado, false));
    await Reject(Request(), "Rechaza marca inactiva");
    await db.Marcas.Where(m => m.IdMarca == 1).ExecuteUpdateAsync(s => s.SetProperty(m => m.Estado, true));
    await db.presentaciones.Where(p => p.IdPresentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, false));
    await Reject(Request(), "Rechaza presentación inactiva");
    await db.presentaciones.Where(p => p.IdPresentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, true));
    bad = Request(); bad.IdsProductos = [1, 1]; await Reject(bad, "Rechaza productos duplicados");
    bad = Request(); bad.IdsProductos = [999]; await Reject(bad, "Rechaza productos inexistentes");
    bad = Request(); bad.IdMarca = 999; await Reject(bad, "Rechaza marca inexistente");
    bad = Request(); bad.Presentaciones[1].IdPresentacion = 1; await Reject(bad, "Rechaza presentaciones duplicadas");
    bad = Request(); bad.Presentaciones[0].UnidadesEquivalentes = 0; await Reject(bad, "Rechaza unidades cero");
    bad = Request(); bad.Presentaciones[0].Precio = 16.201m; await Reject(bad, "Rechaza precio con más de dos decimales");
    await db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, 3.01m));
    await Reject(Request(), "Rechaza costos con partes enteras distintas");
    Check((await service.Consultar(created.IdSurtido, default)).Presentaciones.All(p => p.PrecioMinimoActual is null), "Consulta detecta incompatibilidad posterior sin modificar estado");
    await db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, 2.700001m));
    await Reject(Request(), "No redondea el costo hacia abajo para permitir una pérdida");
    bad = Request(); bad.Presentaciones[0].Precio = 16.21m; bad.Presentaciones[1].Precio = 32.41m;
    Check((await service.Crear(bad, default)).Presentaciones[0].PrecioMinimoActual == 16.21m, "Mínimo con seis decimales se eleva al centavo suficiente");
    await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER impedir_auditoria BEFORE INSERT ON auditoria_evento BEGIN SELECT RAISE(ABORT, 'prueba'); END;");
    var before = await db.Surtidos.CountAsync();
    try { await service.Crear(bad, default); throw new Exception("No revirtió auditoría"); }
    catch (DbUpdateException) { db.ChangeTracker.Clear(); }
    Check(await db.Surtidos.CountAsync() == before && await db.Set<ProductoPresentacion>().CountAsync() == before * 2,
        "Fallo de auditoría revierte surtido, composición y presentaciones");
    Console.WriteLine($"{passed} comprobaciones aprobadas con base SQLite temporal.");
}

sealed class CheckDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        var keep = new HashSet<Type> { typeof(Surtido), typeof(SurtidoProducto), typeof(ProductoPresentacion), typeof(Productos),
            typeof(Marca), typeof(Categoria), typeof(Presentacion), typeof(Usuario), typeof(Rol), typeof(AuditoriaEvento), typeof(AuditoriaEventoDetalle) };
        foreach (var e in b.Model.GetEntityTypes().ToArray()) if (!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
    }
}
