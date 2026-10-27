namespace API_SISTEMA.Models;

public class ZonaEntrega
{
    public int IdZonaEntrega { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal MinimoCompra { get; set; }
    public bool Estado { get; set; } = true;
    public List<PedidoCatalogo> Pedidos { get; set; } = new();
}
