namespace API_SISTEMA.Utilidades;

public sealed class MarcaDuplicadaException : Exception
{
    public MarcaDuplicadaException()
        : base("Ya existe una marca con ese nombre, incluso si está inactiva.") { }
}
