namespace API_SISTEMA.Dtos.Categoria;

// El enlace y el archivo son alternativas, no se permiten ambos.
public sealed class CrearCategoriaConImagenDto : CrearCategoriaDto
{
    public IFormFile? Imagen { get; set; }
}
