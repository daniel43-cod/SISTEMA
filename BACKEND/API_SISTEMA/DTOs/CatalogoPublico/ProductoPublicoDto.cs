namespace API_SISTEMA.Dtos.CatalogoPublico;

public sealed class ProductoPublicoDto
{
    public int IdProducto { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? UrlImagen { get; init; }
    public List<PresentacionPublicaDto> Presentaciones { get; init; } = new();
}

public sealed class PresentacionPublicaDto
{
    public int IdProductoPresentacion { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public bool Disponible { get; init; }
}

public sealed class ProductosPublicosPaginaDto
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public bool HayMas { get; init; }
    public List<ProductoPublicoDto> Productos { get; init; } = new();
}
