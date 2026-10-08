using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Proveedores
    {
        [Key]
        public int id_proveedor { get; set; }
   
        public string nombre { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public string? correo { get; set; }
        public string? direccion { get; set; }
        public string? nit {  get; set; }
        public DateTime fecha_creacion { get; set; }
    }
}
