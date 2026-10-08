namespace API_SISTEMA.Dtos.Marcas;

public sealed class CrearMarcaConImagenDto : CrearMarcaDto
{
    public IFormFile? Imagen { get; set; }
}
