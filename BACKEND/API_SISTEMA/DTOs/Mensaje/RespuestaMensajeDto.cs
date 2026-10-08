namespace API_SISTEMA.Dtos.Mensaje
{
    public class RespuestaMensajeDto
    {
        public int IdMensaje { get; set; }
        public int IdConversacion { get; set; }
        public string? RespuestaMensaje { get; set; }
    }
}
