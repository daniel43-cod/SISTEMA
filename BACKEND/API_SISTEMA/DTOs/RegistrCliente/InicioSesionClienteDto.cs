using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.RegistrCliente
{
    public class InicioSesionClienteDto
    {
        [Required, EmailAddress]
        public required string CorreoElectronico { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class InicioSesionClienteRespuestaDto
    {
        public int IdCuentaCliente { get; set; }
        public required string CorreoElectronico { get; set; }
        public required string Token { get; set; }
    }
}
