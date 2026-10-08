namespace API_SISTEMA.Dtos.Login
{
    public class LoginRespuestaDto
    {
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string rol { get; set; }
        public string token { get; set; }
    }
}
