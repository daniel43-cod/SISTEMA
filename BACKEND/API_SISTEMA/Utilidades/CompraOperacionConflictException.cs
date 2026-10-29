namespace API_SISTEMA.Utilidades;

public sealed class CompraOperacionConflictException() : Exception(
    "La clave de operación ya fue utilizada con otros datos o por otro usuario.");
