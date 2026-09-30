namespace API_SISTEMA.Utilidades;

public sealed class PresentacionDuplicadaException() : Exception(
    "Ya existe una presentación con esa descripción, incluso si está inactiva.");
