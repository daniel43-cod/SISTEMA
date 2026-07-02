using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Productos
    {
        [Key]
        public int id_producto { get; set; }
        public string codigo_barra { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public int id_categoria { get; set; }
        public decimal precio_compra { get; set; }
        public int stock { get; set; }
        public int stock_minimo { get; set; }
        public string? imagen { get; set; }

     // public  int id_tipo_producto { get; set; }
        public decimal impuesto { get; set; } 
        public DateTime fecha_creacion {  get; set; }

        //nevegacion v:
        public List<Detalle_venta> DetalleVentas { get; set; } = new();
        public ICollection<Producto_precio> ProductoPrecios { get; set; } = new List<Producto_precio>();
       
    }
}
