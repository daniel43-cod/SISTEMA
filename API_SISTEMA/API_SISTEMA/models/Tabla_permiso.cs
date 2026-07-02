using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Tabla_permiso
    {
        [Key]
     public int id_permiso {  get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }

        public List<Rol_permisocs> RolPermisos { get; set; }

    }
}
