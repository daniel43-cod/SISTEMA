using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class UsuarioPermiso
    {
        [Key]
        public int id_usuario_permiso {  get; set; }
        public int id_usuario { get; set; }
        public int id_permiso { get; set; }
        public bool permitido { get; set; }   

        public Usuario Usuario { get; set; }
        public TablaPermiso tabla_permiso { get;  set; }
    }
}
