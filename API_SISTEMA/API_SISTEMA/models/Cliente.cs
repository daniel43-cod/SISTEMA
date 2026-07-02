using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Cliente
    {
      
            [Key]
            public int id_Cliente { get; set; }

            public string nombre { get; set; } = string.Empty;
            public string? apellido { get; set; }
            public string? nit { get; set; }
            public string? dpi { get; set; }
            public string? telefono { get; set; }
            public string? correo_electronico { get; set; }
            public string? direccion { get; set; }

            public DateTime fecha_Creacion { get; set; } = DateTime.Now;
          //  public int id_tipo_cliente { get; set; }
            public decimal limite_Credito { get; set; }
            public bool estado { get; set; }

          //  public TipoCliente? TipoCliente { get; set; }
        }
    
}
