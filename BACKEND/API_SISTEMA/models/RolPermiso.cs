using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class RolPermiso
    {
        [Key]
    public int id_rol_permiso {  get; set; }
    public int id_permiso { get; set; }
    public int id_rol { get; set; }

        public TablaPermiso Permiso { get; set; } // navegación hacia el permiso
        public Rol Rol { get; set; } // navegación hacia el rol
    }
}
