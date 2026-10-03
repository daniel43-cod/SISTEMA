namespace API_SISTEMA.DTOs.Productos;

public sealed class CrearProductoConImagenDTO : productocrear
{
    public IFormFile? Imagen { get; set; }
}
