namespace API_SISTEMA.Utilidades;

// Solo contiene errores de negocio seguros para responder al cliente.
public sealed class CajaValidationException(string message) : Exception(message);
