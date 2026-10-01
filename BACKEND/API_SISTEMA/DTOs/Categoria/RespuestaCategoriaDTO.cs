namespace API_SISTEMA.DTOs.Categoria;

public sealed class RespuestaCategoriaDTO
{
    public int IdCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? UrlImagen { get; init; }
    public bool Estado { get; init; }
}

