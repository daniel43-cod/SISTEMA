namespace API_SISTEMA.DTOs.Marcas;

public sealed class ActualizarMarcaConImagenDTO : ActualizarMarcaDTO
{
    public IFormFile? Imagen { get; set; }
}
