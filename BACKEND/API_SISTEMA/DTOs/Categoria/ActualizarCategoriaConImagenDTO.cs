namespace API_SISTEMA.Dtos.Categoria;

public sealed class ActualizarCategoriaConImagenDto : ActualizarCategoriaDto
{
    public IFormFile? Imagen { get; set; }
}
