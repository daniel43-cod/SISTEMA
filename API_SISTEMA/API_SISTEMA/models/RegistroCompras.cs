using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class RegistroCompras
    {
        [Key]
        public int id_compra { get; set; }
        [Required]
        public int id_empresa   { get; set; }
        [Required]
        public int id_usuario { get; set; } 
        [Required]
        public int id_estado_compra { get; set; }
        [Required]
        public DateTime fecha_compra { get; set; }
        [Required]
        public decimal total { get; set; }
    }
}
