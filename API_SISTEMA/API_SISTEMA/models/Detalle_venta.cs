using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.models
{
    public class Detalle_venta
    {
        [Key]
        public int id_detalle_venta {  get; set; }
        public int id_venta { get; set; }
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal? descuento { get; set; }
        public decimal subtotal { get; set; }


        [ForeignKey("id_producto")]
        public Productos Producto { get; set; }

        [ForeignKey("id_venta")]
        public Ventas Venta { get; set; }


    }
}
