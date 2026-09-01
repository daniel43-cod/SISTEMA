using API_SISTEMA.controllers;
using API_SISTEMA.data;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.CompraS;
using API_SISTEMA.services.Gastos;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.services.PagoCompra;
using API_SISTEMA.services.Permisos;
using API_SISTEMA.services.ProductoS;
using API_SISTEMA.services.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//agregue la coneciom del appsetings
builder.Services.AddDbContext<SistemaDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

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

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "No se encontró Jwt:Key.")))
        };
    });
builder.Services.AddAuthorization();
// agregue la clase conexion
builder.Services.AddSingleton<conexion>();
//agregue el services del usuario
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<DetalleVenta_Service>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<RolPermisoService>();
builder.Services.AddScoped<PermisoService>();
builder.Services.AddScoped<ProductoPrecioService>();
builder.Services.AddScoped<LoginService>();
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
builder.Services.AddScoped<ProductoCrearService>();
builder.Services.AddScoped<SubirImagenService>();
builder.Services.AddScoped<BuscarCodigoBarraService>();
//compras
builder.Services.AddScoped<CrearCompraService>();
//ventas
builder.Services.AddScoped<BuscarVentaServices>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.Run();
app.UseStaticFiles();
