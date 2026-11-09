using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.Models
{
    public class Rol
    {
        [Key]
        public int id_rol {  get; set; }
        public required string nombre { get; set; }
        public required string descripcion { get; set; }
        public bool estado { get; set; }
    
    public List<RolPermiso> RolPermisos { get; set; } = new();
    }

}
