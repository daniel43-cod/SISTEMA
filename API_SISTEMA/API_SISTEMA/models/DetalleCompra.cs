using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class DetalleCompra
    {
        [Key]
        public int id_detalle_compra { get; set; }
        public int id_registro_compra { get; set; }
        [Required]
        public decimal subtotal { get; set; }
        [Required]
        public int id_producto { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public decimal precio { get; set; }
    }
}
