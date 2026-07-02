using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Rol_permisocs
    {
        [Key]
    public int id_rol_permiso {  get; set; }
    public int id_permiso { get; set; }
    public int id_rol { get; set; }

        public Tabla_permiso Permiso { get; set; } // navegación hacia el permiso
        public Rol Rol { get; set; } // navegación hacia el rol
    }
}
