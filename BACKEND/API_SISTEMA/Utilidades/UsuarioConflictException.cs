namespace API_SISTEMA.Utilidades;

public sealed class UsuarioConflictException(string message) : Exception(message);
