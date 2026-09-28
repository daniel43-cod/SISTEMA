namespace API_SISTEMA.services.IA;

public sealed class OpenAIException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public int? IdMensaje { get; set; }
    public int? IdConversacion { get; set; }
}
