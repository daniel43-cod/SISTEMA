using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class TablaPermiso
    {
        [Key]
     public int id_permiso {  get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }

        public List<RolPermiso> RolPermisos { get; set; }

    }
}
