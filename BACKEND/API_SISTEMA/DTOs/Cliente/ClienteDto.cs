using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Cliente
{
    public class ClienteDto
    {
      
            [System.Text.Json.Serialization.JsonPropertyName("id_cliente")]
            public int id_Cliente { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string? apellido { get; set; }
            public string? nit { get; set; }
            public string? dpi { get; set; }
            public string? telefono { get; set; }
            public string? correo_electronico { get; set; }
            public string? direccion { get; set; }
           
       
        
        
    }
}
