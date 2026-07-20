using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Producto_Presentacion
    {
        [Key]
        public int id_producto_presentacion { get; set; }
        public int id_producto { get; set; }
        public string descripcion { get; set; }
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
        public bool estado { get; set; }

        public Productos Producto { get; set; }
    }
}
