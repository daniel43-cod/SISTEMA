using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class caja
    {
        [Key]
        public int id_caja { get; set; }
        public string nombre_caja { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }
    }
}
