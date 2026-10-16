namespace API_SISTEMA.Dtos.Conversacion;

public sealed class ChatClienteDto
{
    public int IdConversacion { get; init; }
    public string Contenido { get; init; } = string.Empty;
}
