namespace API_SISTEMA.DTOs.Mensaje
{
    public class RespuestaMensajeDto
    {
        public int IdMensaje { get; set; }
        public int IdConversacion { get; set; }
        public string? RespuestaMensaje { get; set; }
    }
}
