namespace API_SISTEMA.Dtos.Productos;

public sealed class ProductoInventarioDto
{
    public int IdProducto { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal? PrecioPromedioCompra { get; init; }
    public int Stock { get; init; }
    public string? CodigoBarra { get; init; }
    public int IdMarca { get; init; }
    public string Marca { get; init; } = string.Empty;
    public decimal? ValorInventario { get; init; }
}

public sealed class ProductoInventarioPaginaDto
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int Total { get; init; }
    public decimal ValorInventarioTotal { get; init; }
    public int ProductosSinCosto { get; init; }
    public List<ProductoInventarioDto> Items { get; init; } = new();
}
