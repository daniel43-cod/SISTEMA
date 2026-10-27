namespace API_SISTEMA.Dtos.PedidosCatalogo;

public sealed class PedidoCatalogoDetalleDto
{
    public int IdPedido { get; init; }
    public DateTime FechaUtc { get; init; }
    public string NombreCliente { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string? Referencias { get; init; }
    public string? Observacion { get; init; }
    public string ZonaEntrega { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public List<DetallePedidoCatalogoDto> Detalles { get; init; } = new();
}

public sealed class DetallePedidoCatalogoDto
{
    public int IdProductoPresentacion { get; init; }
    public string NombreProducto { get; init; } = string.Empty;
    public string NombrePresentacion { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
}
