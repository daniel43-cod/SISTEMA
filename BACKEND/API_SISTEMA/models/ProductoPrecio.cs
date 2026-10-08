using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class ProductoPrecio
    {
        [Key]
        public int id_producto_precio {  get; set; }
        public int id_producto { get; set; }
        public int id_tipo_cliente { get; set; }
        public decimal precio {  get; set; }
        public bool estado {  get; set; }

        //relacion con productos
        public API_SISTEMA.Models.Productos Producto { get; set; }
        //relacion con tipocliente
        public TipoCliente TipoCliente { get; set; }
 
    }
}
