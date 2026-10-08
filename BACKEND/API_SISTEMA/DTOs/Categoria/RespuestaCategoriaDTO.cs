namespace API_SISTEMA.Dtos.Categoria;

public sealed class RespuestaCategoriaDto
{
    public int IdCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? UrlImagen { get; init; }
    public bool Estado { get; init; }
}

