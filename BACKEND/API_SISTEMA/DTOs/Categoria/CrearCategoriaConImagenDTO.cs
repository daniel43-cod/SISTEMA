namespace API_SISTEMA.DTOs.Categoria;

// El enlace y el archivo son alternativas, no se permiten ambos.
public sealed class CrearCategoriaConImagenDTO : CrearCategoriaDTO
{
    public IFormFile? Imagen { get; set; }
}
