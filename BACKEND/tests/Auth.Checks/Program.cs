using System.Net;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using API_SISTEMA.Controllers;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Services.Productos;
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
builder.Services.AddScoped<PermisoService>();
builder.Services.AddScoped<API_SISTEMA.Services.Usuarios.UsuarioCrearService>();
builder.Services.AddScoped<API_SISTEMA.Services.Usuarios.UsuarioListarService>();
builder.Services.AddScoped<API_SISTEMA.Services.Usuarios.UsuarioEditarService>();
builder.Services.AddScoped<API_SISTEMA.Services.Usuarios.UsuarioEstadoService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<CompraService>();
builder.Services.AddScoped<API_SISTEMA.Services.Gastos.CrearGastosService>();
builder.Services.AddScoped<API_SISTEMA.Services.MovimientoCaja.ListarMovimientoCajaService>();
// Una misma regla de saldo para consultar y cerrar caja.
builder.Services.AddScoped<API_SISTEMA.Services.Caja.CajaSaldoService>();
builder.Services.AddScoped<CajaService>();
builder.Services.AddScoped<API_SISTEMA.Services.Compras.CrearCompraService>();
builder.Services.AddScoped<API_SISTEMA.Services.PagoCompra.PagoCompraService>();
builder.Services.AddScoped<API_SISTEMA.Services.MovimientoCaja.MovimientoCajaService>();
builder.Services.AddScoped<API_SISTEMA.Services.Auditoria.ProductoAuditoriaService>();
builder.Services.AddScoped<ProductoCrearService>();
builder.Services.AddScoped<API_SISTEMA.Services.Productos.ProductoActualizarService>();
builder.Services.AddScoped<API_SISTEMA.Services.Productos.ProductoImagenService>();
builder.Services.AddScoped<API_SISTEMA.Services.Auditoria.CatalogoAuditoriaService>();
builder.Services.AddScoped<API_SISTEMA.Services.Marca.MarcaImagenService>();
builder.Services.AddScoped<API_SISTEMA.Services.Marca.MarcaCrearService>();
builder.Services.AddScoped<API_SISTEMA.Services.Marca.MarcaActualizarService>();
builder.Services.AddScoped<API_SISTEMA.Services.Marca.EstadoMarcaService>();
builder.Services.AddScoped<API_SISTEMA.Services.Categoria.CategoriaImagenService>();
builder.Services.AddScoped<API_SISTEMA.Services.Categoria.CategoriaCrearService>();
builder.Services.AddScoped<API_SISTEMA.Services.Categoria.CategoriaActualizarService>();
builder.Services.AddScoped<API_SISTEMA.Services.Categoria.EstadoCategoriaService>();
builder.Services.AddScoped<API_SISTEMA.Services.Auditoria.PresentacionAuditoriaService>();
builder.Services.AddScoped<API_SISTEMA.Services.Presentaciones.CrearPresentacionService>();
builder.Services.AddScoped<API_SISTEMA.Services.Presentaciones.ActualizarPresentacionService>();
builder.Services.AddScoped<API_SISTEMA.Services.Presentaciones.EstadoPresentacionService>();
builder.Services.AddScoped<API_SISTEMA.Services.Presentaciones.ListarPresentacionService>();
builder.Services.AddScoped<API_SISTEMA.Services.Sesiones.SesionCierreService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<API_SISTEMA.Security.ContextoPeticion>();
builder.Services.AddScoped<API_SISTEMA.Services.Auditoria.AuditoriaService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UsuarioTokenValidator>();
builder.Services.Configure<JwtSettings>(o =>
{
    o.Key = key; o.Issuer = "checks"; o.Audience = "checks"; o.DurationInMinutes = 720;
});
builder.Services.AddControllers().AddApplicationPart(typeof(LoginController).Assembly);
builder.Services.AddRateLimiter(o => { o.AddPolicy<string, ProductoConsultaRateLimitPolicy>("consulta-productos"); o.AddPolicy<string, LoginRateLimitPolicy>("login-interno"); o.AddPolicy<string, UsuarioCrearRateLimitPolicy>("crear-usuarios"); o.AddPolicy<string, UsuarioCrearRateLimitPolicy>("estado-usuarios"); o.AddPolicy<string, CompraRateLimitPolicy>("compras"); o.AddPolicy<string, CajaRateLimitPolicy>("caja"); });
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
builder.Services.AddAuthorization(o => o.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
await using var app = builder.Build();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/checks/sin-atributo", () => "protegido");
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
    db.tabla_Permisos.Add(new TablaPermiso { id_permiso = 1, nombre = "Vender", descripcion = "Ventas", estado = true });
    db.rol_Permisocs.Add(new RolPermiso { id_rol = 2, id_permiso = 1 });
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
async Task PermisosRechazados(HttpStatusCode status)
{
    Check((await client.GetAsync("/api/pedidos-catalogo/1/detalle")).StatusCode == status,
        "Detalle de pedido protegido: " + status);
    Check((await client.GetAsync("/api/pedidos-catalogo/pendientes")).StatusCode == status,
        "Listado de pedidos protegido: " + status);
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
    var antes = await db.rol_Permisocs.AsNoTracking().Select(p => p.id_rol_permiso).ToListAsync();
    Check((await client.GetAsync("/api/Permiso/rol/2")).StatusCode == status, "Consulta permisos rechazada " + status);
    Check((await client.PostAsync("/api/Permiso/asignar?idRol=1&idPermiso=1", null)).StatusCode == status, "Asignar permiso rechazado " + status);
    Check((await client.DeleteAsync("/api/Permiso/quitar?idRol=2&idPermiso=1")).StatusCode == status, "Quitar permiso rechazado " + status);
    var despues = await db.rol_Permisocs.AsNoTracking().Select(p => p.id_rol_permiso).ToListAsync();
    Check(antes.Order().SequenceEqual(despues.Order()), "Rechazo no altera permisos " + status);
}
async Task UpdateAdmin(Action<Usuario> change)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
    var user = await db.usuarios.SingleAsync(u => u.id_usuario == 1);
    change(user); await db.SaveChangesAsync();
}

try
{
    Check((await client.GetAsync("/checks/sin-atributo")).StatusCode == HttpStatusCode.Unauthorized, "Fallback bloquea omisiones");
    await PermisosRechazados(HttpStatusCode.Unauthorized);
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Listado anónimo bloqueado");
    foreach (var route in new[] { "/api/Usuario", "/api/Login/crear" })
        Check((await client.PostAsJsonAsync(route, Account())).StatusCode == HttpStatusCode.Unauthorized, "Alta anónima bloqueada: " + route);

    Check((await client.PostAsJsonAsync("/api/Caja/abrir", new {})).StatusCode == HttpStatusCode.Unauthorized, "Caja bloquea apertura anonima");
    Check((await client.GetAsync("/api/Compra/listar")).StatusCode == HttpStatusCode.Unauthorized, "Compras bloquea consulta anonima");
    Check((await client.PostAsJsonAsync("/api/Compra/crear", new {})).StatusCode == HttpStatusCode.Unauthorized, "Compras bloquea escritura anonima");
    Check((await client.PostAsJsonAsync("/api/Gastos/crear", new {})).StatusCode == HttpStatusCode.Unauthorized,
        "Gastos bloquea escritura anónima");
    Check((await client.GetAsync("/api/MovimientoCaja/listar?idSesionCaja=1")).StatusCode == HttpStatusCode.Unauthorized, "Movimientos bloquea consulta anonima");
    var token = await Token(await Login("admin", "Password123"));
    using (var scope = app.Services.CreateScope())
    {
        var evento = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos.Include(e => e.Detalles).SingleAsync();
        Check(evento.IdUsuario.HasValue && evento.UsuarioResponsable == "admin" && evento.Accion == "SESION_INICIADA" &&
            evento.Resultado == "EXITOSO" && evento.FechaUtc > DateTime.UtcNow.AddMinutes(-1) &&
            !string.IsNullOrWhiteSpace(evento.TraceId) && evento.Detalles.Any(d => d.Campo == "idSesion"),
            "Login guarda responsable validado, fecha y referencia sin credenciales");
    }
    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    foreach (var gastoRoute in new[] { "/api/Gastos/crear", "/CrearGasto" })
    {
        var gastoResponse = await client.PostAsJsonAsync(gastoRoute,
            new { descripcion = "Transporte", monto = 10m, observacion = "Prueba" });
        var gastoBody = await gastoResponse.Content.ReadAsStringAsync();
        Check(gastoResponse.StatusCode == HttpStatusCode.BadRequest && gastoBody.Contains("No hay una caja abierta") &&
            !gastoBody.Contains("detalle") && !gastoBody.Contains("Exception"),
            "Gastos devuelve caja cerrada sin detalles internos: " + gastoRoute);
    }
    Check((await client.PostAsJsonAsync("/api/Gastos/crear",
        new { descripcion = "Transporte", monto = "inválido", observacion = "Prueba" })).StatusCode == HttpStatusCode.BadRequest,
        "Gastos rechaza JSON con monto inválido antes de ejecutar el servicio");
    foreach (var payload in new object[]
    {
        new { descripcion = "   ", monto = 10m },
        new { descripcion = new string('A', 301), monto = 10m },
        new { descripcion = "Transporte", monto = 0m },
        new { descripcion = "Transporte", monto = -1m },
        new { descripcion = "Transporte", monto = 100000000m },
        new { descripcion = "Transporte", monto = 1.001m },
        new { descripcion = "Transporte", monto = 10m, observacion = new string('A', 501) }
    })
    {
        var invalidExpense = await client.PostAsJsonAsync("/api/Gastos/crear", payload);
        var invalidBody = await invalidExpense.Content.ReadAsStringAsync();
        Check(invalidExpense.StatusCode == HttpStatusCode.BadRequest && invalidBody.Contains("errors"),
            "DTO de gastos rechaza datos inválidos antes de consultar caja");
    }
    Check((await client.GetAsync("/api/Compra/listar?tamanoPagina=101")).StatusCode == HttpStatusCode.BadRequest, "Listado de compras limita cantidad solicitada");
    Check((await client.PostAsJsonAsync("/api/Compra/crear", new { id_proveedor = 0, detalle_compra = new object[0] })).StatusCode == HttpStatusCode.BadRequest, "Endpoint valida DTO de compra");
    Check((await client.GetAsync("/api/Compra/detalle/999999")).StatusCode == HttpStatusCode.NotFound, "Detalle devuelve 404 para compra inexistente");
    foreach (var url in new[] { "/api/MovimientoCaja/listar", "/api/MovimientoCaja/listar?idSesionCaja=0", "/api/MovimientoCaja/listar?idSesionCaja=-1", "/api/MovimientoCaja/listar?idSesionCaja=abc" })
        Check((await client.GetAsync(url)).StatusCode == HttpStatusCode.BadRequest, "Movimientos valida parametro de sesion: " + url);
    Check((await client.GetAsync("/api/MovimientoCaja/listar?idSesionCaja=999999")).StatusCode == HttpStatusCode.NotFound, "Endpoint de movimientos devuelve 404");
    var nueva = await client.PostAsJsonAsync("/api/Presentaciones", new { descripcion = "Unidad auditoría" });
    Check(nueva.StatusCode == HttpStatusCode.Created, "Crear presentación auditada");
    var presentacionAuditada = await nueva.Content.ReadFromJsonAsync<API_SISTEMA.Dtos.Presentaciones.PresentacionRespuestaDto>();
    var ruta = "/api/Presentaciones/" + presentacionAuditada!.IdPresentacion;
    Check((await client.PutAsJsonAsync(ruta, new { descripcion = "Caja auditoría" })).IsSuccessStatusCode, "Editar presentación auditada");
    await client.PutAsJsonAsync(ruta, new { descripcion = "Caja auditoría" });
    await client.PatchAsJsonAsync(ruta + "/estado", new { estado = false });
    await client.PatchAsJsonAsync(ruta + "/estado", new { estado = false });
    await client.PatchAsJsonAsync(ruta + "/estado", new { estado = true });
    using (var scope = app.Services.CreateScope())
    {
        var eventos = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos.Include(e => e.Detalles)
            .Where(e => e.Entidad == "presentaciones").OrderBy(e => e.IdAuditoria).ToListAsync();
        Check(eventos.Select(e => e.Accion).SequenceEqual(new[] { "PRESENTACION_CREADA", "PRESENTACION_EDITADA",
            "PRESENTACION_DESACTIVADA", "PRESENTACION_ACTIVADA" }), "Cuatro eventos y sin duplicados al guardar igual");
        Check(eventos.All(e => e.IdUsuario == 1 && e.UsuarioResponsable == "admin" && e.IdRegistro == presentacionAuditada.IdPresentacion.ToString()),
            "Auditoría identifica responsable y presentación");
        Check(eventos[1].Detalles.Single().ValorAnterior == "Unidad auditor\u00eda" && eventos[1].Detalles.Single().ValorNuevo == "Caja auditor\u00eda",
            "Detalle conserva descripcion anterior y nueva");
        Check(eventos[1].Detalles.Single().Campo == "descripcion", "Edicion registra solo descripcion modificada");
        Check(eventos[2].Detalles.Single().Campo == "estado" && eventos[2].Detalles.Single().ValorAnterior == "true" && eventos[2].Detalles.Single().ValorNuevo == "false" &&
            eventos[3].Detalles.Single().ValorAnterior == "false" && eventos[3].Detalles.Single().ValorNuevo == "true",
            "Estados se registran en detalles");
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
    await PermisosRechazados(HttpStatusCode.Forbidden);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var badPassword = await Login("admin", "Wrong123");
    var unknown = await Login("missing", "Wrong123");
    Check(badPassword.StatusCode == HttpStatusCode.Unauthorized && unknown.StatusCode == HttpStatusCode.Unauthorized &&
        await badPassword.Content.ReadAsStringAsync() == await unknown.Content.ReadAsStringAsync(), "Credenciales incorrectas sin enumeración por mensaje");
    Check((await Login("admin", new string('á', 37))).StatusCode == HttpStatusCode.BadRequest, "Límite de contraseña en bytes UTF-8");
    using (var scope = app.Services.CreateScope())
    {
        var eventos = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos.Include(e => e.Detalles).ToListAsync();
        Check(eventos.Count(e => e.Accion == "SESION_INICIADA") == 1,
            "Credenciales rechazadas no generan un evento exitoso");
        var fallidos = eventos.Where(e => e.Accion == "LOGIN_FALLIDO").ToList();
        Check(fallidos.Count == 2 && fallidos.Any(e => e.IdentificadorIntentado == "admin") &&
            fallidos.Any(e => e.IdentificadorIntentado == "missing"), "Cuenta existente y desconocida registran intento fallido");
        Check(fallidos.All(e => e.IdUsuario is null && e.UsuarioResponsable is null && e.IdRegistro is null &&
            e.Resultado == "RECHAZADO" && e.Origen == "API" && !string.IsNullOrWhiteSpace(e.TraceId) &&
            e.Detalles.Count == 0 && e.Motivo == "Autenticación rechazada."),
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
        var evento = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().AuditoriaEventos.Include(e => e.Detalles)
            .SingleAsync(e => e.Accion == "SESION_CERRADA");
        Check(evento.IdUsuario == 1 && evento.UsuarioResponsable == "admin" && evento.Resultado == "EXITOSO" &&
            evento.Motivo == "Cierre voluntario de sesión." && !string.IsNullOrWhiteSpace(evento.TraceId),
            "Auditoría de cierre atribuye responsable autenticado");
    }
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token cerrado no puede volver a usarse");
    using (var scope = app.Services.CreateScope())
        token = (await scope.ServiceProvider.GetRequiredService<LoginService>().Login(
            new API_SISTEMA.Dtos.Login.LoginDto { usuario = "admin", password = "Password123" }))!.token;
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
        Check((await client.PostAsJsonAsync(route, Account(phone: "3000", email: "otro@example.com"))).StatusCode == HttpStatusCode.Conflict, "Usuario duplicado: " + route);
        Check((await client.PostAsJsonAsync(route, Account(username: "otro", phone: "3000"))).StatusCode == HttpStatusCode.Conflict, "Correo duplicado: " + route);
        Check((await client.PostAsJsonAsync(route, Account(username: "otro", email: "otro@example.com"))).StatusCode == HttpStatusCode.Conflict, "Teléfono duplicado: " + route);
    }
    Check((await client.PostAsJsonAsync("/api/Login/crear", Account("segundo", "3000", "segundo@example.com"))).IsSuccessStatusCode, "Alias de alta funcional");
    Check((await client.PostAsJsonAsync("/api/Usuario", new {
        id_rol = 2, nombre = "Sin", apellido = "Correo", usuario = "sin-correo-1", telefono = "4001", password = "Password123"
    })).IsSuccessStatusCode, "Alta sin correo opcional");
    Check((await client.PostAsJsonAsync("/api/Usuario", new {
        id_rol = 2, nombre = "Sin", apellido = "Correo", usuario = "sin-correo-2", telefono = "4002", corre_electronico = "", password = "Password123"
    })).IsSuccessStatusCode, "Segunda alta con correo vacío convertido a null");
    Check((await client.PostAsJsonAsync("/api/Usuario", Account("telefono-invalido", "abc", "telefono@example.com"))).StatusCode == HttpStatusCode.BadRequest,
        "Alta rechaza teléfono sin números");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        var nuevo = await db.usuarios.SingleAsync(u => u.usuario == "sin-correo-2");
        Check(nuevo.correo is null && BCrypt.Net.BCrypt.Verify("Password123", nuevo.password), "Correo vacío normalizado y contraseña hasheada");
        var evento = await db.AuditoriaEventos.Include(e => e.Detalles).SingleAsync(e => e.Accion == "USUARIO_CREADO" && e.IdRegistro == nuevo.id_usuario.ToString());
        Check(evento.IdUsuario == 1 && evento.UsuarioResponsable == "admin", "Auditoría de alta identifica al administrador responsable");
        var detalles = JsonSerializer.Serialize(evento.Detalles);
        Check(!detalles.Contains("Password123") && !detalles.Contains(nuevo.password), "Auditoría de alta sin contraseña ni hash");
    }
    int editId;
    string originalHash;
    DateTime originalDate;
    using (var scope = app.Services.CreateScope())
    {
        var original = await scope.ServiceProvider.GetRequiredService<SistemaDbContext>().usuarios.AsNoTracking().SingleAsync(u => u.usuario == "segundo");
        editId = original.id_usuario; originalHash = original.password; originalDate = original.fecha_Creacion;
    }
    object Edit(string username = "segundo-editado", string phone = "3000", int role = 2) => new {
        id_rol = role, nombre = "Editado", apellido = "Prueba", usuario = username, telefono = phone, corre_electronico = (string?)null
    };
    client.DefaultRequestHeaders.Authorization = null;
    Check((await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit())).StatusCode == HttpStatusCode.Unauthorized, "Edición anónima bloqueada");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    Check((await client.PutAsJsonAsync("/api/Usuario/0", Edit())).StatusCode == HttpStatusCode.BadRequest, "Edición rechaza ID inválido");
    Check((await client.PutAsJsonAsync("/api/Usuario/2147483647", Edit())).StatusCode == HttpStatusCode.NotFound, "Edición de usuario inexistente devuelve 404");
    Check((await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit(role: 3))).StatusCode == HttpStatusCode.BadRequest, "Edición rechaza rol cliente");
    Check((await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit(username: "empleado"))).StatusCode == HttpStatusCode.Conflict, "Edición rechaza usuario duplicado");
    Check((await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit(phone: "4002"))).StatusCode == HttpStatusCode.Conflict, "Edición rechaza teléfono duplicado");
    var editResponse = await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit());
    Check(editResponse.IsSuccessStatusCode && !(await editResponse.Content.ReadAsStringAsync()).Contains("password", StringComparison.OrdinalIgnoreCase), "Edición correcta sin exponer contraseña");
    Check((await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit())).IsSuccessStatusCode, "Edición sin cambios permite conservar valores únicos propios");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        var updated = await db.usuarios.AsNoTracking().SingleAsync(u => u.id_usuario == editId);
        Check(updated.correo is null && updated.password == originalHash && updated.fecha_Creacion == originalDate && updated.estado, "Edición conserva contraseña, fecha y estado; permite quitar correo");
        var events = await db.AuditoriaEventos.Include(e => e.Detalles).Where(e => e.Accion == "USUARIO_EDITADO" && e.IdRegistro == editId.ToString()).ToListAsync();
        Check(events.Count == 1 && events[0].IdUsuario == 1 && !JsonSerializer.Serialize(events[0].Detalles).Contains(originalHash), "Edición audita responsable sin hash ni eventos por guardar igual");
    }
    string oldUserToken;
    using (var scope = app.Services.CreateScope())
        oldUserToken = (await scope.ServiceProvider.GetRequiredService<LoginService>().Login(new API_SISTEMA.Dtos.Login.LoginDto { usuario = "segundo-editado", password = "Password123" }))!.token;
    Check((await client.PatchAsJsonAsync("/api/Usuario/1/estado", new { estado = false })).StatusCode == HttpStatusCode.BadRequest, "Estado impide desactivar la propia cuenta");
    Check((await client.PatchAsJsonAsync($"/api/Usuario/{editId}/estado", new { })).StatusCode == HttpStatusCode.BadRequest, "Estado faltante no se interpreta como false");
    Check((await client.PatchAsJsonAsync("/api/Usuario/2147483647/estado", new { estado = false })).StatusCode == HttpStatusCode.NotFound, "Estado de usuario inexistente devuelve 404");
    var stateResponse = await client.PatchAsJsonAsync($"/api/Usuario/{editId}/estado", new { estado = false });
    Check(stateResponse.IsSuccessStatusCode, "Desactiva usuario: " + stateResponse.StatusCode + " " + await stateResponse.Content.ReadAsStringAsync());
    using (var scope = app.Services.CreateScope())
        Check(await scope.ServiceProvider.GetRequiredService<LoginService>().Login(new API_SISTEMA.Dtos.Login.LoginDto { usuario = "segundo-editado", password = "Password123" }) is null,
            "Usuario desactivado no inicia sesión");
    Check((await client.PatchAsJsonAsync($"/api/Usuario/{editId}/estado", new { estado = true })).IsSuccessStatusCode, "Reactiva usuario");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oldUserToken);
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Reactivar no rehabilita token revocado");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        Check(await db.AuditoriaEventos.CountAsync(e => e.IdRegistro == editId.ToString() && (e.Accion == "USUARIO_ACTIVADO" || e.Accion == "USUARIO_DESACTIVADO") && e.IdUsuario == 1) == 2, "Auditoría de estado identifica al administrador");
    }
    var employeeToken = await Token(await Login("empleado", "Password123"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employeeToken);
    Check((await client.GetAsync("/api/MovimientoCaja/listar?idSesionCaja=1")).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede consultar resumen financiero");
    await PermisosRechazados(HttpStatusCode.Forbidden);
    using (var adminClient = new HttpClient { BaseAddress = client.BaseAddress })
    {
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Check((await adminClient.GetAsync("/api/Permiso/rol/2")).IsSuccessStatusCode, "Administrador consulta permisos");
        Check((await adminClient.PostAsync("/api/Permiso/asignar?idRol=1&idPermiso=1", null)).IsSuccessStatusCode, "Administrador asigna permiso");
        Check((await adminClient.DeleteAsync("/api/Permiso/quitar?idRol=1&idPermiso=1")).IsSuccessStatusCode, "Administrador retira permiso");
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
        var eventos = await db.AuditoriaEventos.Include(e => e.Detalles).Where(e => e.Entidad == "rol_permiso").ToListAsync();
        Check(eventos.Count == 2 && eventos.All(e => e.IdUsuario == 1 && e.UsuarioResponsable == "admin" && e.Detalles.Any(d => d.Campo == "id_permiso")), "Auditoria identifica responsable y permiso");
        Check(eventos.Any(e => e.Accion == "PERMISO_ASIGNADO") && eventos.Any(e => e.Accion == "PERMISO_RETIRADO"), "Auditoria distingue asignar y quitar");
        Check(await db.rol_Permisocs.CountAsync() == 1 && !await db.rol_Permisocs.AnyAsync(p => p.id_rol == 1), "Cambios autorizados persisten");
    }
    Check((await client.PostAsJsonAsync("/api/Caja/abrir", new { id_caja = 1, monto_inicial = 0 })).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede abrir caja");
    Check((await client.PostAsJsonAsync("/api/Caja/cerrar", new { id_sesion_caja = 1, monto_contado = 0 })).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede cerrar caja");
    Check((await client.PostAsJsonAsync("/api/Compra/pago-compra", new { id_compra = 1, monto = 1 })).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede registrar pagos de compras");
    Check((await client.PostAsJsonAsync("/api/Login/crear", Account())).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede crear cuentas");
    Check((await client.PutAsJsonAsync($"/api/Usuario/{editId}", Edit())).StatusCode == HttpStatusCode.Forbidden, "Vendedor no puede editar usuarios");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    await UpdateAdmin(u => u.estado = false);
    Check((await client.GetAsync("/api/Usuario")).StatusCode == HttpStatusCode.Unauthorized, "Token rechazado al desactivar cuenta");
    using (var scope = app.Services.CreateScope())
        Check(await scope.ServiceProvider.GetRequiredService<LoginService>().Login(
            new API_SISTEMA.Dtos.Login.LoginDto { usuario = "admin", password = "Password123" }) is null,
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
        var processor = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Sesiones.SesionCierreService>();
        Check(await processor.ProcesarVencidas() == 1, "Procesador detecta sesión vencida sin navegador");
        Check(await processor.ProcesarVencidas() == 0, "Expiración no se registra dos veces");
        var evento = await db.AuditoriaEventos.Include(e => e.Detalles).SingleAsync(e => e.Accion == "SESION_EXPIRADA");
        Check(evento.IdRegistro == expirada.IdSesion.ToString("D") && evento.Origen == "SISTEMA" &&
            evento.FechaUtc == fecha && evento.IdUsuario == 1, "Auditoría conserva fecha real y sesión expirada");
        await db.Entry(expirada).ReloadAsync();
        Check(expirada.MotivoCierre == "EXPIRACION" && expirada.FechaRevocacion == fecha,
            "Expiración actualiza la sesión");
        Check(await db.AuditoriaEventos.Include(e => e.Detalles).CountAsync(e => e.Accion == "SESION_CERRADA") == 1,
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
            var categoria = await scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Categoria.CategoriaCrearService>()
                .CrearCategoria(new() { Nombre = "Bebidas", UrlImagen = "https://example.com/categoria.png" });
            var marca = await scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Marca.MarcaCrearService>()
                .CrearMarca(new() { Nombre = "Marca inicial", IdCategoria = categoria.IdCategoria });
            var presentacion = new Presentacion { Descripcion = "Unidad producto", Estado = true };
            db.presentaciones.Add(presentacion);
            await db.SaveChangesAsync();
            var producto = await scope.ServiceProvider.GetRequiredService<ProductoCrearService>().CrearProducto(new()
            {
                nombre = "Producto auditoria", codigo_barra = "AUD001", IdMarca = marca.IdMarca,
                presentaciones = [new() { id_presentacion = presentacion.IdPresentacion, unidades_equivalentes = 1, precio = 10m }]
            });
            var asociacion = producto.ProductoPresentaciones.Single();
            var editarProducto = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Productos.ProductoActualizarService>();
            var dtoProducto = new API_SISTEMA.Dtos.Productos.ActualizarProductoDto
            {
                nombre = "Producto nuevo", codigo_barra = "AUD001", IdMarca = marca.IdMarca, stock_minimo = 0,
                presentaciones = [new() { id_producto_presentacion = asociacion.id_producto_presentacion,
                    id_presentacion = presentacion.IdPresentacion, unidades_equivalentes = 1, precio = 12m, estado = true }]
            };
            await editarProducto.ActualizarProducto(producto.IdProducto, dtoProducto);
            await editarProducto.ActualizarProducto(producto.IdProducto, dtoProducto);
            db.proveedores.Add(new Proveedores { id_proveedor = 1, nombre = "Proveedor prueba", telefono = "123", fecha_creacion = DateTime.Now });
            db.estado_compras.AddRange(new EstadoCompra { id_estado_compra = 1, nombre_estado_compra = "Pagada", descripcion = "Pagada" }, new EstadoCompra { id_estado_compra = 2, nombre_estado_compra = "Pendiente", descripcion = "Pendiente" });
            db.caja.Add(new API_SISTEMA.Models.Caja { id_caja = 1, estado = true });
            db.usuarios.Add(new Usuario { id_usuario = 900, id_rol = 1, nombre = "Segundo", apellido = "Admin", usuario = "adminCaja", password = "unused", telefono = "900", estado = true, fecha_Creacion = DateTime.Now });
            db.tipomovimientocaja.AddRange(new TipoMovimientoCaja { id_tipo_movimiento = 14, nombre_movimiento = "Compra", naturaleza = "Salida" }, new TipoMovimientoCaja { id_tipo_movimiento = 18, nombre_movimiento = "Abono", naturaleza = "EGRESO" });
            await db.SaveChangesAsync();
            var cajaService = scope.ServiceProvider.GetRequiredService<CajaService>();
            accessorCatalogo.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "900")], "checks"));
            var sesionCompartida = await cajaService.AbrirCaja(new() { id_caja = 1, monto_inicial = 100m }, 900);
            var dobleApertura = false;
            try { await cajaService.AbrirCaja(new() { id_caja = 1, monto_inicial = 0 }, 900); } catch (CajaValidationException) { dobleApertura = true; }
            Check(dobleApertura && await db.sesioncaja.CountAsync(s => s.fecha_cierre == null) == 1, "Caja rechaza segunda apertura");
            accessorCatalogo.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "checks"));
            var compras = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Compras.CrearCompraService>();
            var compraDto = new API_SISTEMA.Dtos.Compras.RegistroComprasDto
            { id_proveedor = 1, monto_pagado = 20m, detalle_compra = [new() { id_producto = producto.IdProducto, cantidad = 5, precio = 50m }] };
            var compra = await compras.CrearCompra(compraDto, 1);
            Check(compra.IdProveedor == 1 && compra.TotalCompra == 50m && compra.SaldoPendiente == 30m && producto.Stock == 5, "Compra usa proveedor, total de linea y stock correcto");
            Check(producto.PrecioPromedioCompra == 10m, "Primera compra calcula costo por unidad con el total de línea");
            Check(await db.pagosCompras.CountAsync() == 1 && await db.movimientocaja.CountAsync() == 1 && (await db.detalle_compras.SingleAsync()).subtotal == 50m, "Compra confirma pago, detalle y movimiento");
            var pagoService = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.PagoCompra.PagoCompraService>();
            var exceso = false;
            try { await pagoService.AbonarCompra(new() { id_compra = compra.IdCompra, monto = 31m }, 1); } catch (CompraValidationException) { exceso = true; }
            Check(exceso && await db.pagosCompras.CountAsync() == 1, "Abono mayor al saldo rechazado sin guardar pago");
            await pagoService.AbonarCompra(new() { id_compra = compra.IdCompra, monto = 30m }, 1);
            Check(compra.SaldoPendiente == 0 && compra.IdEstadoCompra == 1 && await db.movimientocaja.CountAsync() == 2, "Abono liquida compra y actualiza estado y caja");
            var precision = false;
            compraDto.detalle_compra[0].precio = 1.001m;
            try { await compras.CrearCompra(compraDto, 1); } catch (CompraValidationException) { precision = true; }
            Check(precision && await db.registroCompras.CountAsync() == 1, "Compra rechaza mas de dos decimales");
            var listado = await scope.ServiceProvider.GetRequiredService<CompraService>().listarcompras();
            Check(listado.Single().nombre_proveedor == "Proveedor prueba", "Listado obtiene proveedor");
            var cierreViejo = false;
            try { await cajaService.CerrarCaja(new() { id_sesion_caja = sesionCompartida.id_sesion_caja + 1, monto_contado = 50m }, 1); }
            catch (CajaValidationException) { cierreViejo = true; }
            Check(cierreViejo && await db.sesioncaja.AnyAsync(s => s.fecha_cierre == null), "Caja rechaza cierre de otro turno");
            var resumenService = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.MovimientoCaja.ListarMovimientoCajaService>();
            var antesDeCerrar = (await resumenService.ConsultarResumen(sesionCompartida.id_sesion_caja))!;
            Check(antesDeCerrar.saldo_esperado == 50m, "Resumen previo coincide con compras y abonos");
            db.movimientocaja.Add(new MovimientoCaja { id_sesion_caja = sesionCompartida.id_sesion_caja,
                id_usuario = 1, id_tipo_movimiento = 14, monto = 1m, fecha_movimiento = DateTime.Now });
            await db.SaveChangesAsync();
            var cerrada = await cajaService.CerrarCaja(new() { id_sesion_caja = sesionCompartida.id_sesion_caja, monto_contado = 49m }, 1);
            Check(cerrada.id_usuario_apertura == 900 && cerrada.id_usuario_cierre == 1 && cerrada.monto_esperado == 49m && cerrada.diferencia == 0, "Cierre recalcula e incluye movimientos posteriores al resumen");
            var cajaAuditoria = await db.AuditoriaEventos.Include(e => e.Detalles).Where(e => e.Entidad == "sesion_caja").ToListAsync();
            Check(cajaAuditoria.Count == 2 && cajaAuditoria.Any(e => e.Accion == "CAJA_ABIERTA" && e.IdUsuario == 900) && cajaAuditoria.Any(e => e.Accion == "CAJA_CERRADA" && e.IdUsuario == 1), "Auditoria atribuye apertura y cierre a sus responsables reales");
            compraDto.detalle_compra[0].precio = 50m;
            var operacionCerrada = false;
            try { await compras.CrearCompra(compraDto, 1); } catch (CajaValidationException) { operacionCerrada = true; }
            Check(operacionCerrada, "Caja cerrada impide registrar compras");
            Check((await scope.ServiceProvider.GetRequiredService<CompraService>().listarcompras()).Count == 0,
                "Listado de compras vacío cuando no hay caja abierta");
            await cajaService.AbrirCaja(new() { id_caja = 1, monto_inicial = 50m }, 1);
            Check((await scope.ServiceProvider.GetRequiredService<CompraService>().listarcompras()).Count == 0,
                "Nueva sesión no muestra compras del turno anterior");
            var turnoNuevo = await db.sesioncaja.AsNoTracking().SingleAsync(s => s.fecha_cierre == null);
            var vacio = await resumenService.ConsultarResumen(turnoNuevo.id_sesion_caja);
            Check(vacio is not null && vacio.total_movimientos == 0 && vacio.movimientos.Count == 0 && vacio.total_entradas == 0 && vacio.total_salidas == 0 && vacio.saldo_esperado == 50m, "Resumen de sesion sin movimientos conserva monto inicial");
            Check(await resumenService.ConsultarResumen(999999) is null, "Resumen de sesion inexistente devuelve null");
            var idInvalido = false;
            try { await resumenService.ConsultarResumen(0); } catch (CajaValidationException) { idInvalido = true; }
            Check(idInvalido, "Resumen valida ID antes de consultar");
            db.tipomovimientocaja.Add(new TipoMovimientoCaja { id_tipo_movimiento = 10, nombre_movimiento = "Venta", naturaleza = " entrada " });
            var fechaComun = DateTime.Now.AddMinutes(1);
            var pruebaMovimientos = Enumerable.Range(0, 150).Select(i => new MovimientoCaja
            {
                id_sesion_caja = sesionCompartida.id_sesion_caja, id_usuario = 1,
                id_tipo_movimiento = i % 2 == 0 ? 10 : 14,
                monto = i % 2 == 0 ? 1.25m : 2m, fecha_movimiento = fechaComun
            }).ToList();
            db.movimientocaja.AddRange(pruebaMovimientos);
            db.movimientocaja.Add(new MovimientoCaja { id_sesion_caja = turnoNuevo.id_sesion_caja, id_usuario = 1, id_tipo_movimiento = 10, monto = 999m, fecha_movimiento = fechaComun });
            await db.SaveChangesAsync();
            var resumen = (await resumenService.ConsultarResumen(sesionCompartida.id_sesion_caja))!;
            Check(resumen.total_movimientos == 153 && resumen.movimientos.Count == 100, "Resumen cuenta toda la sesion y limita listado a 100");
            Check(resumen.total_entradas == 93.75m && resumen.total_salidas == 201m && resumen.saldo_esperado == -7.25m,
                "Totales incluyen movimientos fuera de pantalla y excluyen otras sesiones");
            Check(resumen.movimientos.Select(m => m.id_movimiento_caja).SequenceEqual(pruebaMovimientos.OrderByDescending(m => m.id_movimiento_caja).Take(100).Select(m => m.id_movimiento_caja)),
                "Listado ordena por fecha e ID cuando las fechas coinciden");
            var tokenResumen = (await scope.ServiceProvider.GetRequiredService<LoginService>().Login(new API_SISTEMA.Dtos.Login.LoginDto { usuario = "admin", password = "Changed123" }))!.token;
            using var resumenRequest = new HttpRequestMessage(HttpMethod.Get, "/api/MovimientoCaja/listar?idSesionCaja=" + sesionCompartida.id_sesion_caja);
            resumenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResumen);
            resumenRequest.Headers.Add("Accept", "application/json");
            using var resumenResponse = await client.SendAsync(resumenRequest);
            Check(resumenResponse.IsSuccessStatusCode, "Resumen HTTP status " + resumenResponse.StatusCode);
            var resumenHttp = await resumenResponse.Content.ReadFromJsonAsync<API_SISTEMA.Dtos.MovimientoCaja.ResumenMovimientosCajaDto>();
            Check(resumenResponse.IsSuccessStatusCode && resumenHttp!.total_movimientos == 153 && resumenHttp.movimientos.Count == 100 && resumenHttp.saldo_esperado == -7.25m,
                "Endpoint entrega resumen completo y ultimos 100 de sesion cerrada");
            var movimientoInvalido = pruebaMovimientos[0].id_movimiento_caja;
            await db.movimientocaja.Where(m => m.id_movimiento_caja == movimientoInvalido).ExecuteUpdateAsync(set => set.SetProperty(m => m.monto, -1m));
            var rechazoResumen = false;
            try { await resumenService.ConsultarResumen(sesionCompartida.id_sesion_caja); } catch (CajaValidationException) { rechazoResumen = true; }
            Check(rechazoResumen, "Resumen rechaza movimiento invalido sin ocultarlo fuera del listado");
            // Compras sin pago aíslan el promedio de los movimientos de caja ya comprobados.
            var compraPromedio = await compras.CrearCompra(new()
            { id_proveedor = 1, monto_pagado = 0m, detalle_compra = [new() { id_producto = producto.IdProducto, cantidad = 5, precio = 60m }] }, 1);
            Check(producto.Stock == 10 && producto.PrecioPromedioCompra == 11m,
                "Nueva compra pondera inventario anterior y costo total recibido");
            var comprasTurno = await scope.ServiceProvider.GetRequiredService<CompraService>().listarcompras();
            Check(comprasTurno.Count == 1 && comprasTurno[0].id_compra == compraPromedio.IdCompra,
                "Listado incluye compra sin pago del turno activo y excluye otros turnos");
            producto.Stock = 2; // Simula unidades restantes después de vender.
            await db.SaveChangesAsync();
            var compraRestante = await compras.CrearCompra(new()
            { id_proveedor = 1, monto_pagado = 0m, detalle_compra = [new() { id_producto = producto.IdProducto, cantidad = 3, precio = 39m }] }, 1);
            Check(producto.Stock == 5 && producto.PrecioPromedioCompra == 12.2m,
                "Promedio usa stock restante, no cantidades de compras históricas");
            await db.detalle_compras.Where(d => d.id_registro_compra == compraPromedio.IdCompra ||
                d.id_registro_compra == compraRestante.IdCompra).ExecuteDeleteAsync();
            await db.registroCompras.Where(c => c.IdCompra == compraPromedio.IdCompra ||
                c.IdCompra == compraRestante.IdCompra).ExecuteDeleteAsync();
            producto.PrecioPromedioCompra = 0m;
            await db.SaveChangesAsync();
            var compraSinCosto = await compras.CrearCompra(new()
            { id_proveedor = 1, monto_pagado = 0m, detalle_compra = [new() { id_producto = producto.IdProducto, cantidad = 60, precio = 920m }] }, 1);
            Check(producto.Stock == 65 && producto.PrecioPromedioCompra == 15.333333m,
                "Costo previo cero usa total entre unidades recibidas sin diluirlo con stock anterior");
            await db.detalle_compras.Where(d => d.id_registro_compra == compraSinCosto.IdCompra).ExecuteDeleteAsync();
            await db.registroCompras.Where(c => c.IdCompra == compraSinCosto.IdCompra).ExecuteDeleteAsync();
            producto.Stock = 5;
            producto.PrecioPromedioCompra = 10m;
            await db.SaveChangesAsync();
            // Forzar un fallo tardio permite verificar que compra, pago y stock se revierten juntos.
            await db.Database.ExecuteSqlRawAsync("DROP TABLE movimiento_caja");
            compraDto.detalle_compra[0].precio = 50m;
            var revertida = false;
            try { await compras.CrearCompra(compraDto, 1); } catch (DbUpdateException) { revertida = true; db.ChangeTracker.Clear(); }
            Check(revertida && await db.registroCompras.CountAsync() == 1 && await db.pagosCompras.CountAsync() == 2 && (await db.productos.AsNoTracking().SingleAsync()).Stock == 5, "Fallo de movimiento revierte compra, pago y stock");
            Check((await db.productos.AsNoTracking().SingleAsync()).PrecioPromedioCompra == 10m,
                "Fallo de compra conserva el costo promedio anterior");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM pagos_compra");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM detalle_compra");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM registro_compras");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM sesion_caja");
            var eventosProducto = await db.AuditoriaEventos.Include(e => e.Detalles).AsNoTracking().Where(e => e.Entidad == "productos" || e.Entidad == "producto_presentacion").ToListAsync();
            Check(eventosProducto.Count == 4, "Producto y asociacion: creacion y edicion sin duplicar guardados iguales");
            Check(eventosProducto.Single(e => e.Accion == "PRODUCTO_EDITADO").Detalles.Single().Campo == "nombre" && eventosProducto.Single(e => e.Accion == "PRODUCTO_EDITADO").Detalles.Single().ValorNuevo == "Producto nuevo", "Producto registra solo nombre modificado");
            Check(eventosProducto.Single(e => e.Accion == "PRODUCTO_PRESENTACION_EDITADA").Detalles.Single().Campo == "precio" && eventosProducto.Single(e => e.Accion == "PRODUCTO_PRESENTACION_EDITADA").Detalles.Single().ValorNuevo == "12", "Presentacion de producto registra solo precio modificado");
            Check(eventosProducto.All(e => e.IdUsuario == 1), "Producto registra responsable autenticado");
            var editarMarca = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Marca.MarcaActualizarService>();
            await editarMarca.ActualizarMarca(marca.IdMarca, new() { Nombre = "Marca nueva", IdCategoria = categoria.IdCategoria });
            await editarMarca.ActualizarMarca(marca.IdMarca, new() { Nombre = "Marca nueva", IdCategoria = categoria.IdCategoria });
            var editarCategoria = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Categoria.CategoriaActualizarService>();
            await editarCategoria.ActualizarCategoria(categoria.IdCategoria, new() { Nombre = "Bebidas", QuitarImagen = true });
            await editarCategoria.ActualizarCategoria(categoria.IdCategoria, new() { Nombre = "Bebidas" });
            var estadoMarca = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Marca.EstadoMarcaService>();
            await estadoMarca.CambiarEstado(marca.IdMarca, new() { Estado = false });
            await estadoMarca.CambiarEstado(marca.IdMarca, new() { Estado = false });
            await estadoMarca.CambiarEstado(marca.IdMarca, new() { Estado = true });
            var estadoCategoria = scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Categoria.EstadoCategoriaService>();
            await estadoCategoria.CambiarEstado(categoria.IdCategoria, new() { Estado = false });
            await estadoCategoria.CambiarEstado(categoria.IdCategoria, new() { Estado = false });
            await estadoCategoria.CambiarEstado(categoria.IdCategoria, new() { Estado = true });
            var eventos = await db.AuditoriaEventos.Include(e => e.Detalles).AsNoTracking().Where(e => e.Entidad == "marcas" || e.Entidad == "categorias").ToListAsync();
            Check(eventos.Count == 8, "Marca y categor?a: creaci?n, edici?n y estados sin eventos por cambios vac?os");
            Check(eventos.All(e => e.IdUsuario == 1 && e.UsuarioResponsable == "admin"), "Auditor?a de cat?logo identifica al responsable");
            var edicion = eventos.Single(e => e.Accion == "MARCA_EDITADA");
            Check(edicion.Detalles.Single().Campo == "nombre" && edicion.Detalles.Single().ValorAnterior == "Marca inicial" && edicion.Detalles.Single().ValorNuevo == "Marca nueva", "Marca: solo nombre alterado");
            var imagenQuitada = eventos.Single(e => e.Accion == "CATEGORIA_EDITADA");
            Check(imagenQuitada.Detalles.Single().Campo == "urlImagen" && imagenQuitada.Detalles.Single().ValorNuevo is null && imagenQuitada.Detalles.Single().ValorAnterior == "https://example.com/categoria.png", "Quitar imagen conserva URL anterior y null nuevo");
            Check(eventos.Where(e => e.Accion.EndsWith("ACTIVADA")).All(e => e.Detalles.Single().Campo == "estado"), "Estado guarda solo campo alterado");
        }
        finally { accessorCatalogo.HttpContext = null; }
        await db.Database.ExecuteSqlRawAsync("DROP TABLE auditoria_evento_detalle");
        accessorCatalogo.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "checks")) };
        try
        {
            var productoAntes = await db.productos.AsNoTracking().SingleAsync();
            var presentacionId = await db.producto_presentaciones.Select(p => p.IdPresentacion).FirstAsync();
            var falloCrearProducto = false;
            var falloEditarProducto = false;
            try
            {
                await scope.ServiceProvider.GetRequiredService<ProductoCrearService>().CrearProducto(new()
                { nombre = "Rollback producto", codigo_barra = "ROLL001", IdMarca = productoAntes.IdMarca,
                  presentaciones = [new() { id_presentacion = presentacionId, unidades_equivalentes = 1, precio = 10m }] });
            }
            catch (DbUpdateException) { falloCrearProducto = true; db.ChangeTracker.Clear(); }
            try
            {
                await scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Productos.ProductoActualizarService>()
                    .ActualizarProducto(productoAntes.IdProducto, new()
                    { nombre = "Nombre revertido", codigo_barra = productoAntes.CodigoBarra!, IdMarca = productoAntes.IdMarca, stock_minimo = 0 });
            }
            catch (DbUpdateException) { falloEditarProducto = true; db.ChangeTracker.Clear(); }
            Check(falloCrearProducto && await db.productos.CountAsync() == 1, "Fallo de auditoria revierte creacion de producto y asociaciones");
            Check(falloEditarProducto && (await db.productos.AsNoTracking().SingleAsync()).Nombre == productoAntes.Nombre,
                "Fallo de auditoria revierte edicion de producto");
            var categoriasAntes = await db.categorias.CountAsync();
            var marcasAntes = await db.Marcas.CountAsync();
            var categoriaId = await db.categorias.Select(c => c.IdCategoria).FirstAsync();
            var categoriaRevertida = false;
            var marcaRevertida = false;
            try { await scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Categoria.CategoriaCrearService>().CrearCategoria(new() { Nombre = "Rollback categoria" }); }
            catch (DbUpdateException) { categoriaRevertida = true; db.ChangeTracker.Clear(); }
            try { await scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Marca.MarcaCrearService>().CrearMarca(new() { Nombre = "Rollback marca", IdCategoria = categoriaId }); }
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
            await scope.ServiceProvider.GetRequiredService<API_SISTEMA.Services.Presentaciones.CrearPresentacionService>()
                .CrearPresentacion(new API_SISTEMA.Dtos.Presentaciones.CrearPresentacionDto { Descripcion = "Debe revertirse" });
        }
        catch (DbUpdateException) { falloAuditoria = true; }
        finally { accessor.HttpContext = null; }
        Check(falloAuditoria && await db.presentaciones.CountAsync() == cantidadAntes,
            "Fallo de auditoría revierte creación de presentación");
        await db.Database.ExecuteSqlRawAsync("DROP TABLE auditoria_evento");
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
    await using (var gastoDb = new SistemaDbContext(new DbContextOptionsBuilder<SistemaDbContext>()
        .UseSqlite("Data Source=:memory:").Options))
    {
        // Base temporal sin tablas: provoca un fallo interno sin tocar la base del negocio.
        var gastoService = new API_SISTEMA.Services.Gastos.CrearGastosService(gastoDb,
            new API_SISTEMA.Services.MovimientoCaja.MovimientoCajaService(gastoDb));
        foreach (var invalidGasto in new API_SISTEMA.Dtos.Gastos.IngresarGastoDto?[]
        {
            null,
            new() { descripcion = "   ", monto = 1m },
            new() { descripcion = new string('A', 301), monto = 1m },
            new() { descripcion = "Transporte", monto = 0m },
            new() { descripcion = "Transporte", monto = 1.001m },
            new() { descripcion = "Transporte", monto = 100000000m },
            new() { descripcion = "Transporte", monto = 1m, observacion = new string('A', 501) }
        })
        {
            var invalid = false;
            try { await gastoService.CrearGasto(invalidGasto!, 1); }
            catch (System.ComponentModel.DataAnnotations.ValidationException) { invalid = true; }
            Check(invalid, "Servicio de gastos valida antes de acceder a la base de datos");
        }
        using var gastoCancellation = new CancellationTokenSource();
        gastoCancellation.Cancel();
        var gastoCancelled = false;
        try { await gastoService.CrearGasto(new() { descripcion = "Transporte", monto = 1m }, 1, gastoCancellation.Token); }
        catch (OperationCanceledException) { gastoCancelled = true; }
        Check(gastoCancelled, "Servicio de gastos respeta cancelación antes de guardar");
        var gastoController = new GastosController(gastoService,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<GastosController>.Instance);
        gastoController.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "checks"))
            }
        };
        var gastoError = (Microsoft.AspNetCore.Mvc.ObjectResult)await gastoController.CrearGasto(new()
            { descripcion = "Transporte", monto = 10m, observacion = "Prueba" });
        var gastoJson = JsonSerializer.Serialize(gastoError.Value);
        Check(gastoError.StatusCode == 500 && gastoJson.Contains("traceId") && !gastoJson.Contains("Exception") &&
            !gastoJson.Contains("SQLite") && !gastoJson.Contains("detalle"), "Gastos oculta errores técnicos y devuelve 500");
    }
    Console.WriteLine($"{passed} comprobaciones aprobadas.");
}
finally { await app.StopAsync(); }

sealed class AuthDbContext(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entity in modelBuilder.Model.GetEntityTypes().ToArray())
            if (entity.ClrType != typeof(RolPermiso) && entity.ClrType != typeof(TablaPermiso) && entity.ClrType != typeof(Usuario) && entity.ClrType != typeof(Rol) && entity.ClrType != typeof(AuditoriaEventoDetalle) && entity.ClrType != typeof(AuditoriaEvento) && entity.ClrType != typeof(SesionUsuario) && entity.ClrType != typeof(Presentacion) && entity.ClrType != typeof(Marca) && entity.ClrType != typeof(Categoria) && entity.ClrType != typeof(Productos) && entity.ClrType != typeof(ProductoPresentacion) && entity.ClrType != typeof(RegistroCompras) && entity.ClrType != typeof(Proveedores) && entity.ClrType != typeof(DetalleCompra) && entity.ClrType != typeof(PagosCompra) && entity.ClrType != typeof(EstadoCompra) && entity.ClrType != typeof(SesionCaja) && entity.ClrType != typeof(API_SISTEMA.Models.Caja) && entity.ClrType != typeof(MovimientoCaja) && entity.ClrType != typeof(TipoMovimientoCaja))
                modelBuilder.Ignore(entity.ClrType);
        modelBuilder.Entity<SesionUsuario>().Property(s => s.RowVersion).IsRowVersion().HasDefaultValue(new byte[8]);
        // Las expresiones SQL Server se prueban en SQL Server; aquí se verifica el flujo HTTP.
        var detalle = modelBuilder.Entity<AuditoriaEventoDetalle>();
        foreach (var constraint in detalle.Metadata.GetCheckConstraints().ToArray())
            detalle.Metadata.RemoveCheckConstraint(constraint.Name);
        detalle.Property(d => d.ValorAnterior).HasColumnType("TEXT");
        detalle.Property(d => d.ValorNuevo).HasColumnType("TEXT");
        var auditoria = modelBuilder.Entity<AuditoriaEvento>();
        foreach (var constraint in auditoria.Metadata.GetCheckConstraints().ToArray())
            auditoria.Metadata.RemoveCheckConstraint(constraint.Name);
        auditoria.Property(e => e.FechaUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");

    }
}
