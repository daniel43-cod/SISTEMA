namespace API_SISTEMA.Utilidades;

// Solo contiene mensajes de negocio seguros para mostrar al cliente.
public sealed class CompraValidationException(string message) : Exception(message);
