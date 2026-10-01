namespace API_SISTEMA.DTOs.Categoria;

public sealed class ActualizarCategoriaConImagenDTO : ActualizarCategoriaDTO
{
    public IFormFile? Imagen { get; set; }
}
