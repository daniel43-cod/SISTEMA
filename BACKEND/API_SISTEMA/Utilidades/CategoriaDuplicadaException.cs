namespace API_SISTEMA.Utilidades;

// El controlador transforma este error de negocio en HTTP 409.
public sealed class CategoriaDuplicadaException : Exception
{
    public CategoriaDuplicadaException()
        : base("Ya existe una categoría con ese nombre, incluso si está inactiva.")
    {
    }
}
