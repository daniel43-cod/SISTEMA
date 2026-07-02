using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Categoria
    {
        [Key]
        public int id_categoria { get; set; }
       
        public string nombre_categoria { get; set; }
        public string descripcion { get; set; }
        public bool estado {  get; set; }
        public DateTime fecha_Creacion { get; set; }

    }
}
