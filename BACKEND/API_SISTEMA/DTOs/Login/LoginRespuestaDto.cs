namespace API_SISTEMA.Dtos.Login
{
    public class LoginRespuestaDto
    {
        public int id_usuario { get; set; }
        public required string nombre { get; set; }
        public required string rol { get; set; }
        public required string token { get; set; }
    }
}
