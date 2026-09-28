using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.RegistrCliente
{
    public class RegistroCuentaClienteDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public required string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[\s\S]+$", ErrorMessage ="La conntraseña debe de incluir al menos una minuscula, una mayuscula o un numero")]
        public required string Password { get; set; }
    }
}
