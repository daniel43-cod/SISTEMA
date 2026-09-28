using System.ComponentModel.DataAnnotations;


namespace API_SISTEMA.DTOs.Login
{
    public class CrearCuentaDTOs
    {
        [Required(ErrorMessage ="El rol del usuario es obligatorio")]
        public int id_rol { get; set; }
        [Required(ErrorMessage ="El nombre es obligatorio")]
        public required string nombre { get; set; }
        [Required(ErrorMessage ="El apellido del usuario es obligatorio")]
        public required string apellido { get; set; }
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public required string usuario { get; set; }
       // [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public  string? corre_electronico { get;set;  }
        [Required(ErrorMessage = "El numero de telefono es obligatorio")]
        public required string telefono { get;set;  }
        [Required(ErrorMessage = "El numero de telefono es obligatorio")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[\s\S]+$", ErrorMessage ="La conntraseña debe de incluir al menos una minuscula, una mayuscula o un numero")]
        public required string password { get; set; }

        
    }
}
