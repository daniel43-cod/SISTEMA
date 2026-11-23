using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace API_SISTEMA.Data;

// Permite generar migraciones sin iniciar JWT, servicios externos ni el servidor HTTP.
public sealed class SistemaDbContextFactory : IDesignTimeDbContextFactory<SistemaDbContext>
{
    public SistemaDbContext CreateDbContext(string[] args)
    {
        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<SistemaDbContext>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var cadena = configuracion.GetConnectionString("ConexionPostgres");
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Configura ConnectionStrings:ConexionPostgres en User Secrets o variables de entorno.");
        return new SistemaDbContext(new DbContextOptionsBuilder<SistemaDbContext>()
            .UseNpgsql(cadena, o => o.SetPostgresVersion(18, 0)).Options);
    }
}