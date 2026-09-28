namespace API_SISTEMA.DTOs.RegistrCliente
{
    public class CuentaClienteCreadaDto
    {
        public int IdCuentaCliente { get; set; }
        public required string CorreoElectronico { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
