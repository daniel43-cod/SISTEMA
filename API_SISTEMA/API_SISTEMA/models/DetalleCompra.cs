using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class DetalleCompra
    {
        [Key]
        public int id_detalle_compra { get; set; }
        [Required]
        public int id_compra { get; set; }
        [Required]
        public int id_producto { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public decimal precio { get; set; }
    }
}
