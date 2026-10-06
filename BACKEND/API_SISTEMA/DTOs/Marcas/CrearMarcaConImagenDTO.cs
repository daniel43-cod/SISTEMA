namespace API_SISTEMA.DTOs.Marcas;

public sealed class CrearMarcaConImagenDTO : CrearMarcaDTO
{
    public IFormFile? Imagen { get; set; }
}
