namespace API_SISTEMA.Dtos.CatalogoPublico;

public sealed class CategoriaPublicaDto
{
    public int IdCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? UrlImagen { get; init; }
}
