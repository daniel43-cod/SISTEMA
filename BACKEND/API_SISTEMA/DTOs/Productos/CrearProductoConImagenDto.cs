namespace API_SISTEMA.Dtos.Productos;

public sealed class CrearProductoConImagenDto : CrearProductoDto
{
    public IFormFile? Imagen { get; set; }
}
