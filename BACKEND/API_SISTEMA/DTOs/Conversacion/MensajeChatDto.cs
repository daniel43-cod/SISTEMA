namespace API_SISTEMA.Dtos.Conversacion;

public sealed class MensajeChatDto
{
    public int IdMensaje { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public string? Respuesta { get; init; }
}
