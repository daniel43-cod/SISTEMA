using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.models
{
    public class Rol
    {
        [Key]
        public int id_rol {  get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }
    
    public List<Rol_permisocs> RolPermisos { get; set; }
    }

}
