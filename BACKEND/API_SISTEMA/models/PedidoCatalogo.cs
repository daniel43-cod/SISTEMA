namespace API_SISTEMA.Models;

public enum EstadoPedidoCatalogo : byte
{
    Pendiente = 0,
    Aceptado = 1,
    Rechazado = 2,
    Cancelado = 3,
    ConvertidoAVenta = 4
}

public class PedidoCatalogo
{
    public int IdPedido { get; set; }
    public Guid IdOperacion { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Referencias { get; set; }
    public string? Observacion { get; set; }
    public int IdZonaEntrega { get; set; }
    public DateTime FechaUtc { get; set; }
    public decimal Total { get; set; }
    public EstadoPedidoCatalogo Estado { get; set; } = EstadoPedidoCatalogo.Pendiente;
    public int? IdVenta { get; set; }

    public ZonaEntrega ZonaEntrega { get; set; } = null!;
    public Ventas? Venta { get; set; }
    public List<DetallePedidoCatalogo> Detalles { get; set; } = new();
}
