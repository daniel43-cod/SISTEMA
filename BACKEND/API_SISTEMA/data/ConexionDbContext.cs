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
                _cadenaConexion = configuration.GetConnectionString("ConexionSQL");
            }

            public SqlConnection CrearConexion()
            {
                return new SqlConnection(_cadenaConexion);
            }
        }

    }
}
