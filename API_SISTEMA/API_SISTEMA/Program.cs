using API_SISTEMA.controllers;
using API_SISTEMA.data;
using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//agregue la coneciom del appsetings
builder.Services.AddDbContext<SistemaDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
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
builder.Services.AddScoped<Pagos>();    

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
app.UseStaticFiles();
