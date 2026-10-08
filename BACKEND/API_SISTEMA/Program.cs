using API_SISTEMA.controllers;
using API_SISTEMA.data;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.IA;
using API_SISTEMA.services.Conversacion;
using API_SISTEMA.services.CuentaCliente;
using API_SISTEMA.services.CompraS;
using API_SISTEMA.services.Gastos;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.services.PagoCompra;
using API_SISTEMA.services.Permisos;
using API_SISTEMA.services.ProductoS;
using API_SISTEMA.services.Prestacion;
using API_SISTEMA.services.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using API_SISTEMA.services.Categoria;

var builder = WebApplication.CreateBuilder(args);
//agregue la coneciom del appsetings
builder.Services.AddDbContext<SistemaDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(settings => Encoding.UTF8.GetByteCount(settings.Key) >= 32,
        "Configura Jwt:Key con una clave aleatoria de al menos 32 bytes fuera del repositorio.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer) &&
        !string.IsNullOrWhiteSpace(settings.Audience) && settings.DurationInMinutes > 0,
        "La configuración JWT es incompleta.")
    .ValidateOnStart();
builder.Services.AddScoped<UsuarioTokenValidator>();
builder.Services.AddRateLimiter(options =>
{
    // Protege apertura, cierre e historial contra solicitudes excesivas.
    options.AddPolicy<string, CajaRateLimitPolicy>("caja");
    options.AddPolicy<string, CompraRateLimitPolicy>("compras");
    options.AddPolicy<string, LoginRateLimitPolicy>("login-interno");
    options.AddPolicy<string, ProductoConsultaRateLimitPolicy>("consulta-productos");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa únicamente el token JWT."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "No se encontró Jwt:Key.")))
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context => context.HttpContext.RequestServices
                .GetRequiredService<UsuarioTokenValidator>().ValidateAsync(context)
        };
    });
builder.Services.AddAuthorization();
// agregue la clase conexion
builder.Services.AddSingleton<conexion>();
//agregue el services del usuario
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<CrearPresentacionServices>();
builder.Services.AddScoped<ListarPresentacionServices>();
builder.Services.AddScoped<ActualizarPresentacionService>();
builder.Services.AddScoped<EstadoPresentacionService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<RegistroCuentaClienteService>();
builder.Services.AddScoped<InicioSesionService>();
builder.Services.AddScoped<ConversacionService>();
builder.Services.AddScoped<CatalogoIAService>();
builder.Services.AddHttpClient<OpenAIService>(client => client.Timeout = TimeSpan.FromSeconds(65));
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<DetalleVenta_Service>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<RolPermisoService>();
builder.Services.AddScoped<PermisoService>();
builder.Services.AddScoped<ProductoPrecioService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.PresentacionAuditoriaService>();
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.CatalogoAuditoriaService>();
builder.Services.AddScoped<API_SISTEMA.services.Sesiones.SesionCierreService>();
builder.Services.AddHostedService<API_SISTEMA.services.Sesiones.SesionExpiracionWorker>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<API_SISTEMA.Securyti.ContextoPeticion>();
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.AuditoriaService>();
builder.Services.AddScoped<PagoService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<CompraService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<CajaService>();
builder.Services.AddScoped<Pago>();
builder.Services.AddScoped<CrearGastosService>();
builder.Services.AddScoped<AbonarSaldoVentaServices>();
builder.Services.AddScoped<MovimientoCajaService>();
builder.Services.AddScoped<ListarMovimientoCajaService>();
builder.Services.AddScoped<CrearVentaService>();
builder.Services.AddScoped<PermisoService>();
builder.Services.AddScoped<PermisoUsuarioService>();
//productos
builder.Services.AddScoped<API_SISTEMA.services.Auditoria.ProductoAuditoriaService>();
builder.Services.AddScoped<ProductoCrearService>();
builder.Services.AddScoped<ProductoActualizarService>();
builder.Services.AddScoped<ProductoBuscarAdminService>();
builder.Services.AddScoped<ProductoImagenService>();
builder.Services.AddScoped<SubirImagenService>();
builder.Services.AddScoped<BuscarCodigoBarraService>();
//compras
builder.Services.AddScoped<CrearCompraService>();
//ventas
builder.Services.AddScoped<BuscarVentaServices>();
builder.Services.AddScoped<ActualizarVentaService>();

//categoria
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaImagenService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaCrearService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaActualizarService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.CategoriaListarService>();
builder.Services.AddScoped<API_SISTEMA.services.Categoria.EstadoCategoriaService>();

//marcas
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaListarService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.EstadoMarcaService>();

// Marcas: servicio de creación con una instancia por petición.
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaCrearService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaImagenService>();
builder.Services.AddScoped<API_SISTEMA.services.Marca.MarcaActualizarService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseStaticFiles();
app.MapControllers();
app.Run();
