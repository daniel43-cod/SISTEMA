using System.Net;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using API_SISTEMA.controllers;
using API_SISTEMA.data;
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

// Base temporal: nunca usa appsettings ni la conexión SQL Server del negocio.
await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
const string key = "auth-checks-only-key-not-for-production-123456789";
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddScoped<SistemaDbContext>(_ => new AuthDbContext(options));
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.CatalogoAuditoriaService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaImagenService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaCrearService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaActualizarService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.EstadoMarcaService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaImagenService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaCrearService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaActualizarService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.EstadoCategoriaService>();
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.PresentacionAuditoriaService>();
builder.Services.AddScoped<API_SISTEMA.services.Prestacion.CrearPresentacionServices>();
builder.Services.AddScoped<API_SISTEMA.services.Prestacion.ActualizarPresentacionService>();
builder.Services.AddScoped<API_SISTEMA.services.Prestacion.EstadoPresentacionService>();
builder.Services.AddScoped<API_SISTEMA.services.Prestacion.ListarPresentacionServices>();
builder.Services.AddScoped<API_SISTEMA.services.Sesiones.SesionCierreService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<API_SISTEMA.Securyti.ContextoPeticion>();
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.AuditoriaService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UsuarioTokenValidator>();
builder.Services.Configure<JwtSettings>(o =>
{
    o.Key = key; o.Issuer = "checks"; o.Audience = "checks"; o.DurationInMinutes = 720;
});
builder.Services.AddControllers().AddApplicationPart(typeof(LoginController).Assembly);
builder.Services.AddRateLimiter(o => o.AddPolicy<string, LoginRateLimitPolicy>("login-interno"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true, ValidIssuer = "checks", ValidAudience = "checks",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromSeconds(30)
    };
    o.Events = new JwtBearerEvents
    {
        OnTokenValidated = c => c.HttpContext.RequestServices.GetRequiredService<UsuarioTokenValidator>().ValidateAsync(c)
    };
});
builder.Services.AddAuthorization();
await using var app = builder.Build();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
    await db.Database.EnsureCreatedAsync();
    db.rols.AddRange(
        new Rol { id_rol = 1, nombre = Roles.Administrador, estado = true, descripcion = "Admin" },
        new Rol { id_rol = 2, nombre = Roles.Vendedor, estado = true, descripcion = "Vendedor" },
        new Rol { id_rol = 3, nombre = "CLIENTE", estado = true, descripcion = "Cliente" });
    db.usuarios.Add(new Usuario
    {
        id_usuario = 1, id_rol = 1, nombre = "Admin", apellido = "Prueba", usuario = "admin",
        password = BCrypt.Net.BCrypt.HashPassword("Password123"), telefono = "1000", estado = true,
        fecha_Creacion = DateTime.Now
    });
    await db.SaveChangesAsync();
}

await app.StartAsync();
var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var client = new HttpClient { BaseAddress = new Uri(address) };
int passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FALLÓ: " + name);
    passed++; Console.WriteLine("OK: " + name);
}
async Task<HttpResponseMessage> Login(string user, string password) =>
    await client.PostAsJsonAsync("/api/Login/Login", new { usuario = user, password });
async Task<string> Token(HttpResponseMessage response)
{
    response.EnsureSuccessStatusCode();
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement.GetProperty("token").GetString()!;
}
object Account(string username = "empleado", string phone = "2000", string email = "empleado@example.com",
    string password = "Password123", int role = 2) => new
    {
        nombre = "Empleado", apellido = "Prueba", usuario = username, telefono = phone,
        corre_electronico = email, password, id_rol = role,
        estado = false, fecha_Creacion = "2000-01-01T00:00:00"
    };
async Task UpdateAdmin(Action<Usuario> change)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
    var user = await db.usuarios.SingleAsync(u => u.id_usuario == 1);
    change(user); await db.SaveChangesAsync();
}

try
{
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Listado anónimo bloqueado");
    foreach (var route in new[] { "/api/Usuario", "/api/Login/crear" })
        Check((await client.PostAsJsonAsync(route, Account())).StatusCode == HttpStatusCode.Unauthorized, "Alta anónima bloqueada: " + route);

    var token = await Token(await Login("admin", "Password123"));
    using (var scope = app.Services.CreateScope())
    {
        var evento = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos.SingleAsync();
        Check(evento.IdUsuario.HasValue && evento.UsuarioResponsable == "admin" && evento.Accion == "SESION_INICIADA" &&
            evento.Resultado == "EXITOSO" && evento.FechaUtc > DateTime.UtcNow.AddMinutes(-1) &&
            !string.IsNullOrWhiteSpace(evento.TraceId) && evento.DatosNuevos!.Contains("idSesion") && evento.DatosAnteriores is null,
            "Login guarda responsable validado, fecha y referencia sin credenciales");
    }
    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var nueva = await client.PostAsJsonAsync("/api/Presentaciones", new { descripcion = "Unidad auditoría" });
    Check(nueva.StatusCode == HttpStatusCode.Created, "Crear presentación auditada");
    var presentacionAuditada = await nueva.Content.ReadFromJsonAsync<API_SISTEMA.DTOs.Presentaciones.PresentacionRespuestaDTO>();
    var ruta = "/api/Presentaciones/" + presentacionAuditada!.IdPresentacion;
    Check((await client.PutAsJsonAsync(ruta, new { descripcion = "Caja auditoría" })).IsSuccessStatusCode, "Editar presentación auditada");
    await client.PutAsJsonAsync(ruta, new { descripcion = "Caja auditoría" });
    await client.PatchAsJsonAsync(ruta + "/estado", new { estado = false });
    await client.PatchAsJsonAsync(ruta + "/estado", new { estado = false });
    await client.PatchAsJsonAsync(ruta + "/estado", new { estado = true });
    using (var scope = app.Services.CreateScope())
    {
        var eventos = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos
            .Where(e => e.Entidad == "presentaciones").OrderBy(e => e.IdAuditoria).ToListAsync();
        Check(eventos.Select(e => e.Accion).SequenceEqual(new[] { "PRESENTACION_CREADA", "PRESENTACION_EDITADA",
            "PRESENTACION_DESACTIVADA", "PRESENTACION_ACTIVADA" }), "Cuatro eventos y sin duplicados al guardar igual");
        Check(eventos.All(e => e.IdUsuario == 1 && e.UsuarioResponsable == "admin" && e.IdRegistro == presentacionAuditada.IdPresentacion.ToString()),
            "Auditoría identifica responsable y presentación");
        Check(eventos[1].DatosAnteriores!.Contains("Unidad auditor") && eventos[1].DatosNuevos!.Contains("Caja auditor"),
            "Auditoría conserva descripción anterior y nueva");
        Check(eventos[1].DatosAnteriores == "{\"descripcion\":\"Unidad auditor\\u00EDa\"}" &&
            !eventos[1].DatosNuevos!.Contains("estado"), "Edición registra solo descripción modificada");
        Check(eventos[2].DatosAnteriores == "{\"estado\":true}" && eventos[2].DatosNuevos == "{\"estado\":false}" &&
            eventos[3].DatosAnteriores == "{\"estado\":false}" && eventos[3].DatosNuevos == "{\"estado\":true}",
            "Activación y desactivación registran solo estado modificado");
    }
    Check(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(61), "Sesión interna respeta el máximo actual de 60 minutos");
    Check(jwt.Claims.All(c => !c.Value.Contains("Password123") && !c.Value.StartsWith("$2")), "JWT sin contraseña ni hash en sus claims");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var listing = await client.GetStringAsync("/api/Usuario");
    Check(!listing.Contains("password", StringComparison.OrdinalIgnoreCase) && !listing.Contains("$2"), "Listado sin hashes");
    var legacy = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken("checks", "checks",
        [new(System.Security.Claims.ClaimTypes.Role, Roles.Administrador),
         new("sub", "1"), new("tipo_cuenta", "usuario")], expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(legacy));
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token antiguo requiere nuevo login");
    var customer = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken("checks", "checks",
        [new(System.Security.Claims.ClaimTypes.Role, "CLIENTE"), new("sub", "1"), new("tipo_cuenta", "cuenta_cliente")],
        expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: legacy.SigningCredentials);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(customer));
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Forbidden, "Cliente con mismo ID no hereda permisos internos");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var badPassword = await Login("admin", "Wrong123");
    var unknown = await Login("missing", "Wrong123");
    Check(badPassword.StatusCode == HttpStatusCode.Unauthorized && unknown.StatusCode == HttpStatusCode.Unauthorized &&
        await badPassword.Content.ReadAsStringAsync() == await unknown.Content.ReadAsStringAsync(), "Credenciales incorrectas sin enumeración por mensaje");
    Check((await Login("admin", new string('á', 37))).StatusCode == HttpStatusCode.BadRequest, "Límite de contraseña en bytes UTF-8");
    using (var scope = app.Services.CreateScope())
    {
        var eventos = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos.ToListAsync();
        Check(eventos.Count(e => e.Accion == "SESION_INICIADA") == 1,
            "Credenciales rechazadas no generan un evento exitoso");
        var fallidos = eventos.Where(e => e.Accion == "LOGIN_FALLIDO").ToList();
        Check(fallidos.Count == 2 && fallidos.Any(e => e.IdentificadorIntentado == "admin") &&
            fallidos.Any(e => e.IdentificadorIntentado == "missing"), "Cuenta existente y desconocida registran intento fallido");
        Check(fallidos.All(e => e.IdUsuario is null && e.UsuarioResponsable is null && e.IdRegistro is null &&
            e.Resultado == "RECHAZADO" && e.Origen == "API" && !string.IsNullOrWhiteSpace(e.TraceId) &&
            e.DatosAnteriores is null && e.DatosNuevos is null && e.Motivo == "Autenticación rechazada."),
            "Intentos no atribuyen identidad ni guardan contraseña, token o detalles de cuenta");
    }

    client.DefaultRequestHeaders.Authorization = null;
    Check((await client.PostAsync("/api/Login/cerrar-sesion", null)).StatusCode == HttpStatusCode.Unauthorized,
        "Cierre voluntario exige autenticación");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    Check((await client.PostAsync("/api/Login/cerrar-sesion", null)).StatusCode == HttpStatusCode.NoContent,
        "Cierre voluntario autenticado devuelve 204");
    using (var scope = app.Services.CreateScope())
    {
        var evento = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos
            .SingleAsync(e => e.Accion == "SESION_CERRADA");
        Check(evento.IdUsuario == 1 && evento.UsuarioResponsable == "admin" && evento.Resultado == "EXITOSO" &&
            evento.Motivo == "Cierre voluntario de sesión." && !string.IsNullOrWhiteSpace(evento.TraceId),
            "Auditoría de cierre atribuye responsable autenticado");
    }
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token cerrado no puede volver a usarse");
    using (var scope = app.Services.CreateScope())
        token = (await scope.ServiceProvider.GetRequiredService<LoginService>().Login(
            new API_SISTEMA.DTOs.Login.LoginDTOs { usuario = "admin", password = "Password123" }))!.token;
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    foreach (var route in new[] { "/api/Usuario", "/api/Login/crear" })
    {
        Check((await client.PostAsJsonAsync(route, Account(password: "a"))).StatusCode == HttpStatusCode.BadRequest, "Contraseña débil bloqueada: " + route);
        Check((await client.PostAsJsonAsync(route, Account(role: 0))).StatusCode == HttpStatusCode.BadRequest, "Rol cero bloqueado: " + route);
        Check((await client.PostAsJsonAsync(route, Account(role: 3))).StatusCode == HttpStatusCode.BadRequest, "Rol cliente bloqueado: " + route);
    }
    var created = await client.PostAsJsonAsync("/api/Usuario", Account());
    var createdBody = await created.Content.ReadAsStringAsync();
    Check(created.IsSuccessStatusCode && !createdBody.Contains("password") && !createdBody.Contains("$2"), "Alta sin hash en respuesta");
    using (var json = JsonDocument.Parse(createdBody))
        Check(json.RootElement.GetProperty("estado").GetBoolean() &&
            json.RootElement.GetProperty("fecha_Creacion").GetDateTime().Year == DateTime.Now.Year, "Estado y fecha definidos por servidor");
    foreach (var route in new[] { "/api/Usuario", "/api/Login/crear" })
    {
        Check((await client.PostAsJsonAsync(route, Account(phone: "3000", email: "otro@example.com"))).StatusCode == HttpStatusCode.BadRequest, "Usuario duplicado: " + route);
        Check((await client.PostAsJsonAsync(route, Account(username: "otro", phone: "3000"))).StatusCode == HttpStatusCode.BadRequest, "Correo duplicado: " + route);
        Check((await client.PostAsJsonAsync(route, Account(username: "otro", email: "otro@example.com"))).StatusCode == HttpStatusCode.BadRequest, "Teléfono duplicado: " + route);
    }
    Check((await client.PostAsJsonAsync("/api/Login/crear", Account("segundo", "3000", "segundo@example.com"))).IsSuccessStatusCode, "Alias de alta funcional");
    var employeeToken = await Token(await Login("empleado", "Password123"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employeeToken);
    Check((await client.PostAsJsonAsync("/api/Login/crear", Account())).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede crear cuentas");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    await UpdateAdmin(u => u.estado = false);
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token rechazado al desactivar cuenta");
    using (var scope = app.Services.CreateScope())
        Check(await scope.ServiceProvider.GetRequiredService<LoginService>().Login(
            new API_SISTEMA.DTOs.Login.LoginDTOs { usuario = "admin", password = "Password123" }) is null,
            "Cuenta inactiva no inicia sesión");
    await UpdateAdmin(u => { u.estado = true; u.id_rol = 2; });
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token rechazado al cambiar rol");
    await UpdateAdmin(u => { u.id_rol = 1; u.password = BCrypt.Net.BCrypt.HashPassword("Changed123"); });
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token rechazado al cambiar contraseña");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        var fecha = DateTime.UtcNow.AddMinutes(-1);
        var expirada = new SesionUsuario { IdSesion = Guid.NewGuid(), IdUsuario = 1,
            FechaCreacion = fecha.AddMinutes(-30), FechaVencimiento = fecha,
            TokenHash = System.Security.Cryptography.SHA256.HashData(Guid.NewGuid().ToByteArray()) };
        db.SesionesUsuario.Add(expirada);
        await db.SaveChangesAsync();
        var processor = scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Sesiones.SesionCierreService>();
        Check(await processor.ProcesarVencidas() == 1, "Procesador detecta sesión vencida sin navegador");
        Check(await processor.ProcesarVencidas() == 0, "Expiración no se registra dos veces");
        var evento = await db.AuditoriaEventos.SingleAsync(e => e.Accion == "SESION_EXPIRADA");
        Check(evento.IdRegistro == expirada.IdSesion.ToString("D") && evento.Origen == "SISTEMA" &&
            evento.FechaUtc == fecha && evento.IdUsuario == 1, "Auditoría conserva fecha real y sesión expirada");
        await db.Entry(expirada).ReloadAsync();
        Check(expirada.MotivoCierre == "EXPIRACION" && expirada.FechaRevocacion == fecha,
            "Expiración actualiza la sesión");
        Check(await db.AuditoriaEventos.CountAsync(e => e.Accion == "SESION_CERRADA") == 1,
            "Cierre voluntario no se convierte en expiración");
    }
    HttpResponseMessage? limited = null;
    for (int i = 0; i < 11; i++)
    {
        limited = await Login("missing", "Wrong123");
        if (limited.StatusCode == HttpStatusCode.TooManyRequests) break;
    }
    Check(limited!.StatusCode == HttpStatusCode.TooManyRequests && limited.Headers.RetryAfter is not null, "Límite de intentos HTTP 429 y Retry-After");

    // Simula un fallo de almacenamiento real; nunca debe devolver el detalle SQL.
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employeeToken);
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        var accessorCatalogo = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        accessorCatalogo.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "checks")) };
        try
        {
            var categoria = await scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Categoria.CategoriaCrearService>()
                .CrearCategoria(new() { Nombre = "Bebidas", UrlImagen = "https://example.com/categoria.png" });
            var marca = await scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Marca.MarcaCrearService>()
                .CrearMarca(new() { Nombre = "Marca inicial", IdCategoria = categoria.IdCategoria });
            var editarMarca = scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Marca.MarcaActualizarService>();
            await editarMarca.ActualizarMarca(marca.IdMarca, new() { Nombre = "Marca nueva", IdCategoria = categoria.IdCategoria });
            await editarMarca.ActualizarMarca(marca.IdMarca, new() { Nombre = "Marca nueva", IdCategoria = categoria.IdCategoria });
            var editarCategoria = scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Categoria.CategoriaActualizarService>();
            await editarCategoria.ActualizarCategoria(categoria.IdCategoria, new() { Nombre = "Bebidas", QuitarImagen = true });
            await editarCategoria.ActualizarCategoria(categoria.IdCategoria, new() { Nombre = "Bebidas" });
            var estadoMarca = scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Marca.EstadoMarcaService>();
            await estadoMarca.CambiarEstado(marca.IdMarca, new() { Estado = false });
            await estadoMarca.CambiarEstado(marca.IdMarca, new() { Estado = false });
            await estadoMarca.CambiarEstado(marca.IdMarca, new() { Estado = true });
            var estadoCategoria = scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Categoria.EstadoCategoriaService>();
            await estadoCategoria.CambiarEstado(categoria.IdCategoria, new() { Estado = false });
            await estadoCategoria.CambiarEstado(categoria.IdCategoria, new() { Estado = false });
            await estadoCategoria.CambiarEstado(categoria.IdCategoria, new() { Estado = true });
            var eventos = await db.AuditoriaEventos.AsNoTracking().Where(e => e.Entidad == "marcas" || e.Entidad == "categorias").ToListAsync();
            Check(eventos.Count == 8, "Marca y categor?a: creaci?n, edici?n y estados sin eventos por cambios vac?os");
            Check(eventos.All(e => e.IdUsuario == 1 && e.UsuarioResponsable == "admin"), "Auditor?a de cat?logo identifica al responsable");
            var edicion = eventos.Single(e => e.Accion == "MARCA_EDITADA");
            Check(edicion.DatosAnteriores == "{\"nombre\":\"Marca inicial\"}" && edicion.DatosNuevos == "{\"nombre\":\"Marca nueva\"}", "Edici?n de marca registra solo nombre alterado");
            var imagenQuitada = eventos.Single(e => e.Accion == "CATEGORIA_EDITADA");
            Check(imagenQuitada.DatosNuevos == "{\"urlImagen\":null}" && !imagenQuitada.DatosAnteriores!.Contains("nombre"), "Quitar imagen registra solo URL anterior y null nuevo");
            Check(eventos.Where(e => e.Accion.EndsWith("ACTIVADA")).All(e => e.DatosNuevos == "{\"estado\":true}" || e.DatosNuevos == "{\"estado\":false}"), "Cambios de estado registran solo estado");
        }
        finally { accessorCatalogo.HttpContext = null; }
        await db.Database.ExecuteSqlRawAsync("DROP TABLE auditoria_evento");
        accessorCatalogo.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "checks")) };
        try
        {
            var categoriasAntes = await db.categorias.CountAsync();
            var marcasAntes = await db.Marcas.CountAsync();
            var categoriaId = await db.categorias.Select(c => c.IdCategoria).FirstAsync();
            var categoriaRevertida = false;
            var marcaRevertida = false;
            try { await scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Categoria.CategoriaCrearService>().CrearCategoria(new() { Nombre = "Rollback categoria" }); }
            catch (DbUpdateException) { categoriaRevertida = true; db.ChangeTracker.Clear(); }
            try { await scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Marca.MarcaCrearService>().CrearMarca(new() { Nombre = "Rollback marca", IdCategoria = categoriaId }); }
            catch (DbUpdateException) { marcaRevertida = true; db.ChangeTracker.Clear(); }
            Check(categoriaRevertida && await db.categorias.CountAsync() == categoriasAntes, "Fallo de auditoria revierte categoria");
            Check(marcaRevertida && await db.Marcas.CountAsync() == marcasAntes, "Fallo de auditoria revierte marca");
        }
        finally { accessorCatalogo.HttpContext = null; }
        var cantidadAntes = await db.presentaciones.CountAsync();
        var accessor = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        accessor.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "checks"))
        };
        var falloAuditoria = false;
        try
        {
            await scope.ServiceProvider.GetRequiredService<API_SISTEMA.services.Prestacion.CrearPresentacionServices>()
                .CrearPresentacion(new API_SISTEMA.DTOs.Presentaciones.CrearPresentacionDTO { Descripcion = "Debe revertirse" });
        }
        catch (DbUpdateException) { falloAuditoria = true; }
        finally { accessor.HttpContext = null; }
        Check(falloAuditoria && await db.presentaciones.CountAsync() == cantidadAntes,
            "Fallo de auditoría revierte creación de presentación");
        await db.Database.ExecuteSqlRawAsync("DROP TABLE sesiones_usuario");
        await db.Database.ExecuteSqlRawAsync("DROP TABLE usuario");
    }
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Fallo de almacenamiento deniega sesión");

    var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(new Microsoft.AspNetCore.Http.DefaultHttpContext(),
        new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
    var exceptionContext = new Microsoft.AspNetCore.Mvc.Filters.ExceptionContext(actionContext, [])
    { Exception = new Exception("SQL password=secret internal stack") };
    new UsuarioExceptionFilter(Microsoft.Extensions.Logging.Abstractions.NullLogger<UsuarioExceptionFilter>.Instance).OnException(exceptionContext);
    var result = (Microsoft.AspNetCore.Mvc.ObjectResult)exceptionContext.Result!;
    var errorBody = JsonSerializer.Serialize(result.Value);
    Check(result.StatusCode == 500 && !errorBody.Contains("secret") && !errorBody.Contains("SQL"), "Error inesperado sin datos internos");
    Console.WriteLine($"{passed} comprobaciones aprobadas.");
}
finally { await app.StopAsync(); }

sealed class AuthDbContext(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entity in modelBuilder.Model.GetEntityTypes().ToArray())
            if (entity.ClrType != typeof(Usuario) && entity.ClrType != typeof(Rol) && entity.ClrType != typeof(AuditoriaEvento) && entity.ClrType != typeof(SesionUsuario) && entity.ClrType != typeof(Presentacion) && entity.ClrType != typeof(Marca) && entity.ClrType != typeof(Categoria))
                modelBuilder.Ignore(entity.ClrType);
        modelBuilder.Entity<SesionUsuario>().Property(s => s.RowVersion).IsRowVersion().HasDefaultValue(new byte[8]);
        // Las expresiones SQL Server se prueban en SQL Server; aquí se verifica el flujo HTTP.
        var auditoria = modelBuilder.Entity<AuditoriaEvento>();
        foreach (var constraint in auditoria.Metadata.GetCheckConstraints().ToArray())
            auditoria.Metadata.RemoveCheckConstraint(constraint.Name);
        auditoria.Property(e => e.FechaUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        auditoria.Property(e => e.DatosAnteriores).HasColumnType("TEXT");
        auditoria.Property(e => e.DatosNuevos).HasColumnType("TEXT");
        modelBuilder.Entity<Rol>().Ignore(r => r.RolPermisos);
    }
}
