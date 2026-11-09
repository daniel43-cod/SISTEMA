using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Data
{
    public class ConexionDbContext : DbContext
    {
        public class Conexion
        {
            
            private readonly string _cadenaConexion;
            
            public Conexion(IConfiguration configuration)
{
    var cadena = configuration.GetConnectionString("ConexionSQL");

    if (string.IsNullOrWhiteSpace(cadena))
        throw new InvalidOperationException(
            "No está configurada la cadena de conexión ConexionSQL.");

    _cadenaConexion = cadena;
}

            public SqlConnection CrearConexion()
            {
                return new SqlConnection(_cadenaConexion);
            }
        }

    }
}
