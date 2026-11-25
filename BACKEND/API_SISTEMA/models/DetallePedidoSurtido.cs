namespace API_SISTEMA.Models;

public sealed class DetallePedidoSurtido
{
    public int IdDetallePedido { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public DetallePedidoCatalogo Detalle { get; set; } = null!;
    public Productos Producto { get; set; } = null!;
}
