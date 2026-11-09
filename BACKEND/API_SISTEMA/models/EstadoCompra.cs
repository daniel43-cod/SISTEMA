using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class EstadoCompra
    {
        [Key]
        public int id_estado_compra { get; set; }
        [Required]
        public required string nombre_estado_compra { get; set; }
        public required string descripcion { get; set; }
    }
}
