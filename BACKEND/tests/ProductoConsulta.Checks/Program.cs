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
        new Productos { id_producto = 2, nombre = "Agua", Marca = marca, stock = null },
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
