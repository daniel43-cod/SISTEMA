using API_SISTEMA.Data;
using API_SISTEMA.Services.Conversacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var root = @"C:\Users\coy81\Desktop\SISTEMA\SISTEMA\BACKEND";
var configuration = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(root, "API_SISTEMA", "appsettings.json"))
    .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Microsoft", "UserSecrets", "129edc18-0e43-4354-9fb9-c9a63b053f5e", "secrets.json"), optional: true)
    .AddEnvironmentVariables().Build();
await using var db = new SistemaDbContext(new DbContextOptionsBuilder<SistemaDbContext>()
    .UseSqlServer(configuration.GetConnectionString("ConexionSQL"), o => o.CommandTimeout(15)).Options);
try
{
    var account = await db.CuentaClientes.AsNoTracking().Where(c => c.Estado).Select(c => (int?)c.IdCuentaCliente).FirstOrDefaultAsync();
    if (account is null) { Console.WriteLine("No hay cuentas activas."); return; }
    // Diagnóstico local de solo lectura: no imprime cuentas, tokens ni contenido.
    var service = new ConversacionListarService(db);
    try { var chats = await service.ListarChats(account.Value); Console.WriteLine($"Consulta actual: OK ({chats.Count} chats)."); }
    catch (Exception ex) { Console.WriteLine("Consulta actual: " + ex.GetType().Name + ": " + ex.Message); }
    var key = configuration["Jwt:Key"];
    var jwt = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
        new[] { new Claim("sub", account.Value.ToString()), new Claim("tipo_cuenta", "cuenta_cliente"), new Claim(ClaimTypes.Role, "CLIENTE") },
        expires: DateTime.UtcNow.AddMinutes(2),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)), SecurityAlgorithms.HmacSha256));
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
    using var response = await client.GetAsync("http://localhost:5272/api/Conversaciones");
    Console.WriteLine("API en ejecución: " + (int)response.StatusCode);
    if (!response.IsSuccessStatusCode)
    {
        var body = await response.Content.ReadAsStringAsync();
        var firstLine = body.Split('\n').FirstOrDefault() ?? "";
        if (firstLine.Contains("Exception")) Console.WriteLine(firstLine);
    }
}
catch (Exception ex) { Console.WriteLine("Diagnóstico: " + ex.GetType().Name + ": " + ex.Message); }
