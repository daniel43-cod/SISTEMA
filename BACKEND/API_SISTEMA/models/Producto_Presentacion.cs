using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.models
{
    public class Producto_Presentacion
    {
        [Key]
        public int id_producto_presentacion { get; set; }
        public int id_producto { get; set; }
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
        public bool estado { get; set; }
        [Column("id_presentacion")]
        public int IdPresentacion {get;set;}

        public Productos Producto { get; set; }
        public Presentacion Presentacion {get;set;}
    }
}
