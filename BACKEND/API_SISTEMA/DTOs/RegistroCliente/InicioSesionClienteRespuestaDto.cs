using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.RegistroCliente;

public class InicioSesionClienteRespuestaDto
    {
        public int IdCuentaCliente { get; set; }
        public required string CorreoElectronico { get; set; }
        public required string Token { get; set; }
    }
