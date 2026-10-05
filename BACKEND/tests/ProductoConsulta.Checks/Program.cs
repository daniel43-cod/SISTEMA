using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using API_SISTEMA.controllers;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.ProductoS;
using API_SISTEMA.services.Prestacion;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

// JWT de prueba; la validación de cuentas/credenciales se cubre en Auth.Checks.
const string key = "producto-consulta-tests-key-at-least-32-bytes";
await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddScoped<SistemaDbContext>(_ => new CatalogoDbContext(options));
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ProductoActualizarService>();
builder.Services.AddScoped<CrearPresentacionServices>();
builder.Services.AddScoped<ListarPresentacionServices>();
builder.Services.AddScoped<ActualizarPresentacionService>();
builder.Services.AddScoped<EstadoPresentacionService>();
builder.Services.AddControllers().AddApplicationPart(typeof(ProductosController).Assembly).AddControllersAsServices();
builder.Services.AddTransient<ProductosController>(sp => new ProductosController(
    sp.GetRequiredService<ProductoService>(), null!, null!, null!,
    sp.GetRequiredService<ILogger<ProductosController>>()));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true, ValidIssuer = "checks", ValidAudience = "checks",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o => o.AddPolicy<string, ProductoConsultaRateLimitPolicy>("consulta-productos"));
await using var app = builder.Build();
app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
    await db.Database.EnsureCreatedAsync();
    var categoria = new Categoria { IdCategoria = 1, nombreCategoria = "Bebidas", Estado = true };
    var marca = new Marca { IdMarca = 1, Nombre = "Marca A", Estado = true, Categoria = categoria };
    var unidad = new Presentacion { IdPresentacion = 1, Descripcion = "Unidad", Estado = true };
    var caja = new Presentacion { IdPresentacion = 2, Descripcion = "Caja", Estado = true };
    var producto = new Productos { id_producto = 1, nombre = "Agua", codigo_barra = "001", Marca = marca, stock = 25, costo_unitario = 777m };
    db.AddRange(categoria, marca, unidad, caja, producto,
        new Productos { id_producto = 2, nombre = "Agua 2", codigo_barra = "002", Marca = marca, stock = null },
        new Producto_Presentacion { id_producto_presentacion = 1, Producto = producto, Presentacion = unidad, estado = true, unidades_equivalentes = 1, precio = 5 },
        new Producto_Presentacion { id_producto_presentacion = 2, Producto = producto, Presentacion = caja, estado = true, unidades_equivalentes = 12, precio = 50 },
        new Producto_Presentacion { id_producto_presentacion = 3, Producto = producto, Presentacion = caja, estado = false, unidades_equivalentes = 0, precio = 50 });
    await db.SaveChangesAsync();
}
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient { BaseAddress = new Uri(address) };
    var passed = 0;
    void Check(bool success, string name) { if (!success) throw new Exception(name); passed++; Console.WriteLine("OK: " + name); }
    void Login(string role)
    {
        var jwt = new JwtSecurityToken("checks", "checks", [new Claim(ClaimTypes.Role, role)],
            expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
    }
    foreach (var url in new[] { "/api/Productos/listar", "/api/Productos/1" })
        Check((await client.GetAsync(url)).StatusCode == HttpStatusCode.Unauthorized, "Anónimo bloqueado: " + url);
    foreach (var role in new[] { "VENDEDOR", "CLIENTE" })
    {
        Login(role);
        foreach (var url in new[] { "/api/Productos/listar", "/api/Productos/1" })
            Check((await client.GetAsync(url)).StatusCode == HttpStatusCode.Forbidden, role + " bloqueado: " + url);
    }
    Login("ADMINISTRADOR");
    var pageResponse = await client.GetAsync("/api/Productos/listar?tamanoPagina=1");
    var page = (await pageResponse.Content.ReadFromJsonAsync<ProductoPaginaDTO>())!;
    Check(pageResponse.IsSuccessStatusCode && page.Total == 2 && page.Items.Count == 1 && page.Items[0].IdProducto == 1,
        "Una fila por producto, conteo y orden estable");
    Check(pageResponse.Headers.CacheControl?.NoStore == true, "Respuesta sin almacenamiento en caché");
    var second = (await client.GetFromJsonAsync<ProductoPaginaDTO>("/api/Productos/listar?tamanoPagina=1&pagina=2"))!;
    Check(second.Items.Single().IdProducto == 2 && second.Items[0].StockUnidades == 0, "Segunda página, producto sin presentaciones y stock nulo");
    var filtered = (await client.GetFromJsonAsync<ProductoPaginaDTO>("/api/Productos/listar?texto=001&idMarca=1&idCategoria=1"))!;
    Check(filtered.Total == 1 && filtered.Items.Single().IdProducto == 1, "Filtros por código, marca y categoría");
    foreach (var query in new[] { "pagina=0", "pagina=100001", "tamanoPagina=101", "tamanoPagina=-1", "idMarca=0", "idCategoria=-1", "pagina=abc", "texto=" + new string('x', 101) })
        Check((await client.GetAsync("/api/Productos/listar?" + query)).StatusCode == HttpStatusCode.BadRequest, "Entrada inválida: " + query[..Math.Min(35, query.Length)]);
    var injection = (await client.GetFromJsonAsync<ProductoPaginaDTO>("/api/Productos/listar?texto=" + Uri.EscapeDataString("' OR 1=1 --")))!;
    Check(injection.Total == 0, "Texto SQL tratado como búsqueda literal");
    var detailResponse = await client.GetAsync("/api/Productos/1");
    var detailBody = await detailResponse.Content.ReadAsStringAsync();
    if (!detailResponse.IsSuccessStatusCode) throw new Exception(detailBody);
    var detail = (await detailResponse.Content.ReadFromJsonAsync<ProductoDetalleDTO>())!;
    Check(detail.Marca == "Marca A" && detail.Categoria == "Bebidas" && detail.Presentaciones.Count == 3 &&
        detail.Presentaciones.Single(p => p.IdProductoPresentacion == 2).PresentacionesDisponibles == 2 &&
        detail.Presentaciones.Single(p => p.IdProductoPresentacion == 3).PresentacionesDisponibles == 0,
        "Detalle, equivalencias y presentación inactiva sin división por cero");
    Check(!detailBody.Contains("costo", StringComparison.OrdinalIgnoreCase) && !detailBody.Contains("777"), "Sin costos internos");
    Check((await client.GetAsync("/api/Productos/0")).StatusCode == HttpStatusCode.BadRequest, "ID inválido");
    Check((await client.GetAsync("/api/Productos/999")).StatusCode == HttpStatusCode.NotFound, "Producto inexistente");
    var edit = new ActualizarProductoDTO { codigo_barra = "001", nombre = "Agua", IdMarca = 1, stock_minimo = 3 };
    client.DefaultRequestHeaders.Authorization = null;
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.Unauthorized, "Edición anónima bloqueada");
    Login("VENDEDOR");
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.Forbidden, "Edición de vendedor bloqueada");
    Login("ADMINISTRADOR");
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).IsSuccessStatusCode, "Conserva nombre y código propios");
    edit.codigo_barra = " 002 ";
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.Conflict, "Código duplicado rechazado");
    edit.codigo_barra = "001"; edit.nombre = " agua 2 ";
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.Conflict, "Nombre duplicado rechazado sin distinguir mayúsculas y bordes");
    edit.nombre = "Agua"; edit.IdMarca = 999;
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.BadRequest, "Marca inexistente bloqueada");
    edit.IdMarca = 1;
    Check((await client.PutAsJsonAsync("/api/Productos/999", edit)).StatusCode == HttpStatusCode.NotFound, "Edición inexistente");
    Check((await client.PutAsJsonAsync("/api/Productos/1", new { codigo_barra = "001", nombre = "Agua", idMarca = 1, stock_minimo = 3, stock = 999 })).StatusCode == HttpStatusCode.BadRequest, "No acepta modificar stock por edición");
    edit.nombre = "Agua renovada"; edit.codigo_barra = "ABC01";
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).IsSuccessStatusCode, "Actualización válida");
    edit.codigo_barra = " aBc01 "; edit.nombre = "Otro";
    Check((await client.PutAsJsonAsync("/api/Productos/2", edit)).StatusCode == HttpStatusCode.Conflict, "Código duplicado alfanumérico sin distinguir mayúsculas");
    var updated = (await client.GetFromJsonAsync<ProductoDetalleDTO>("/api/Productos/1"))!;
    Check(updated.Nombre == "Agua renovada" && updated.CodigoBarra == "ABC01" && updated.StockMinimo == 3 &&
        updated.StockUnidades == 25 && updated.Presentaciones.Count == 3, "Guarda edición y conserva stock y presentaciones");
    edit.codigo_barra = "ABC01"; edit.nombre = "Agua renovada";
    edit.presentaciones = [new() { id_producto_presentacion = 1, id_presentacion = 1, unidades_equivalentes = 2, precio = 8.50m, estado = true }];
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).IsSuccessStatusCode, "Edita equivalencia y precio y desactiva omitidas");
    updated = (await client.GetFromJsonAsync<ProductoDetalleDTO>("/api/Productos/1"))!;
    Check(updated.Presentaciones.Count == 3 && updated.Presentaciones.Single(p => p.IdProductoPresentacion == 1).Precio == 8.50m &&
        updated.Presentaciones.Single(p => p.IdProductoPresentacion == 1).UnidadesEquivalentes == 2 &&
        !updated.Presentaciones.Single(p => p.IdProductoPresentacion == 2).Activa, "Conserva IDs y registros desactivados");
    edit.presentaciones[0].id_producto_presentacion = 999;
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.BadRequest, "ID ajeno o inexistente rechazado");
    edit.presentaciones[0].id_producto_presentacion = 1;
    edit.presentaciones[0].id_presentacion = 2;
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.BadRequest, "No cambia identidad histórica de presentación");
    edit.presentaciones[0].id_presentacion = 1; edit.presentaciones[0].precio = 8.501m;
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.BadRequest, "Precio con más de dos decimales rechazado");
    edit.presentaciones[0].precio = 8.5m;
    edit.presentaciones.Add(edit.presentaciones[0]);
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.BadRequest, "Presentaciones repetidas rechazadas");
    edit.presentaciones.RemoveAt(1);
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        db.presentaciones.Add(new Presentacion { IdPresentacion = 3, Descripcion = "Paquete", Estado = true });
        db.producto_presentaciones.Add(new Producto_Presentacion { id_producto = 2, IdPresentacion = 3, estado = true, unidades_equivalentes = 1, precio = 5 });
        await db.SaveChangesAsync();
        edit.presentaciones[0].id_producto_presentacion = await db.producto_presentaciones.Where(p => p.id_producto == 2).Select(p => p.id_producto_presentacion).SingleAsync();
    }
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).StatusCode == HttpStatusCode.BadRequest, "No edita presentación de otro producto");
    edit.presentaciones[0].id_producto_presentacion = 1;
    edit.presentaciones.Add(new() { id_presentacion = 3, unidades_equivalentes = 6, precio = 25, estado = true });
    Check((await client.PutAsJsonAsync("/api/Productos/1", edit)).IsSuccessStatusCode, "Agrega presentación nueva");
    updated = (await client.GetFromJsonAsync<ProductoDetalleDTO>("/api/Productos/1"))!;
    Check(updated.Presentaciones.Count == 4 && updated.Presentaciones.Single(p => p.IdPresentacion == 3).PresentacionesDisponibles == 4,
        "Nueva asociación con ID y disponibilidad");
    client.DefaultRequestHeaders.Authorization = null;
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { estado = false })).StatusCode == HttpStatusCode.Unauthorized, "Estado anónimo bloqueado");
    Login("VENDEDOR");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { estado = false })).StatusCode == HttpStatusCode.Forbidden, "Estado vendedor bloqueado");
    Check((await client.GetAsync("/api/Presentaciones/administracion")).StatusCode == HttpStatusCode.Forbidden, "Listado administrativo vendedor bloqueado");
    Login("ADMINISTRADOR");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { })).StatusCode == HttpStatusCode.BadRequest, "Estado obligatorio");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/0/estado", new { estado = false })).StatusCode == HttpStatusCode.BadRequest, "ID de estado inválido");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/999/estado", new { estado = false })).StatusCode == HttpStatusCode.NotFound, "Estado inexistente");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { estado = false, descripcion = "Cambio" })).StatusCode == HttpStatusCode.BadRequest, "Estado rechaza campos ajenos");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { estado = false })).IsSuccessStatusCode, "Desactivación por administrador");
    var activeCatalog = await client.GetFromJsonAsync<List<API_SISTEMA.DTOs.Presentaciones.PresentacionRespuestaDTO>>("/api/Presentaciones");
    var adminCatalog = await client.GetFromJsonAsync<List<API_SISTEMA.DTOs.Presentaciones.PresentacionRespuestaDTO>>("/api/Presentaciones/administracion");
    Check(activeCatalog!.All(p => p.IdPresentacion != 1) && adminCatalog!.Any(p => p.IdPresentacion == 1 && !p.Estado), "Inactivas visibles solo en catálogo administrativo");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { estado = false })).IsSuccessStatusCode, "Desactivación repetida idempotente");
    Check((await client.PatchAsJsonAsync("/api/Presentaciones/1/estado", new { estado = true })).IsSuccessStatusCode, "Reactivación por administrador");
    HttpResponseMessage? limited = null;
    for (var i = 0; i < 61; i++)
    {
        limited = await client.GetAsync("/api/Productos/999");
        if (limited.StatusCode == HttpStatusCode.TooManyRequests) break;
    }
    Check(limited!.StatusCode == HttpStatusCode.TooManyRequests && limited.Headers.RetryAfter is not null,
        "Límite compartido de consultas con HTTP 429 y Retry-After");
    // Otra cuenta tiene una partición independiente y permite probar errores SQL.
    var independent = new JwtSecurityToken("checks", "checks", [new Claim(ClaimTypes.Role, "ADMINISTRADOR"), new Claim(ClaimTypes.NameIdentifier, "2")],
        expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(independent));
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().Database.ExecuteSqlRawAsync("DROP TABLE productos");
    foreach (var url in new[] { "/api/Productos/listar", "/api/Productos/1" })
    {
        var failure = await client.GetAsync(url);
        var body = await failure.Content.ReadAsStringAsync();
        Check(failure.StatusCode == HttpStatusCode.InternalServerError && body.Contains("traceId") && !body.Contains("SQLite"), "Error interno controlado: " + url);
    }
    Console.WriteLine($"{passed} comprobaciones aprobadas, sin SQL Server real.");
}
finally { await app.StopAsync(); }

sealed class CatalogoDbContext(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var keep = new[] { typeof(Productos), typeof(Marca), typeof(Categoria), typeof(Producto_Presentacion), typeof(Presentacion) };
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            if (!keep.Contains(entity.ClrType)) builder.Ignore(entity.ClrType);
        builder.Entity<Productos>().Ignore(p => p.DetalleVentas).Ignore(p => p.ProductoPrecios);
    }
}
