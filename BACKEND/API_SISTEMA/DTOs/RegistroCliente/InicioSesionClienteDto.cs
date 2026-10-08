using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.RegistroCliente
{
    public class InicioSesionClienteDto
    {
        [Required, EmailAddress]
        public required string CorreoElectronico { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    
}
