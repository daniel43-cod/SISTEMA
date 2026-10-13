namespace API_SISTEMA.Dtos.CatalogoPublico;

public sealed class MarcaPublicaDto
{
    public int IdMarca { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? UrlImagen { get; init; }
}
