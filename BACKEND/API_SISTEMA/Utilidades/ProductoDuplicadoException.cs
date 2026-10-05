namespace API_SISTEMA.Utilidades;

public sealed class ProductoDuplicadoException : Exception
{
    public ProductoDuplicadoException() : base("Ya existe un producto con ese código de barras.") { }
    public ProductoDuplicadoException(string mensaje) : base(mensaje) { }
}
