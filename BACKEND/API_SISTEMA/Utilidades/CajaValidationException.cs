namespace API_SISTEMA.Utilidades;

// Solo contiene errores de negocio seguros para responder al cliente.
public sealed class CajaValidationException(string message, string? codigo = null) : Exception(message)
{
    public string? Codigo { get; } = codigo;
}
