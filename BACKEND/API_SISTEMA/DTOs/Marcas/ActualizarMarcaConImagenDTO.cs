namespace API_SISTEMA.Dtos.Marcas;

public sealed class ActualizarMarcaConImagenDto : ActualizarMarcaDto
{
    public IFormFile? Imagen { get; set; }
}
