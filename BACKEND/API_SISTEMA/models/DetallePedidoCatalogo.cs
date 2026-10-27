namespace API_SISTEMA.Models;

public class DetallePedidoCatalogo
{
    public int IdDetallePedido { get; set; }
    public int IdPedido { get; set; }
    public int IdProductoPresentacion { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombrePresentacion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    // Calculado por SQL Server; EF lo recupera al guardar.
    public decimal Subtotal { get; private set; }

    public PedidoCatalogo Pedido { get; set; } = null!;
    public ProductoPresentacion ProductoPresentacion { get; set; } = null!;
}
