using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class Usuario
    {
        [Key]
        public int id_usuario {  get; set; }
        public int id_rol {  get; set; }
        public required string nombre { get; set; }
        public required string apellido { get; set; }
        public required string usuario { get; set; }
        public string? correo { get; set; }
        public required string telefono { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public string password { get; set; } = "";
        public bool estado { get; set; }
        public  DateTime fecha_Creacion {  get; set; }
        public  Rol rol { get; set; }
    }
}
