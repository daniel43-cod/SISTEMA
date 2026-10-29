namespace API_SISTEMA.Dtos.Compras;

public sealed class PagoCompraHistorialDto
{
    public int IdPago { get; set; }
    public int IdCompra { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal? Monto { get; set; }
    public string? Observacion { get; set; }
    public string Usuario { get; set; } = "";
}
