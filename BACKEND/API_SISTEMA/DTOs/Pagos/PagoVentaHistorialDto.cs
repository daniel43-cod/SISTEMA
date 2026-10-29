namespace API_SISTEMA.Dtos.Pagos;

public sealed class PagoVentaHistorialDto
{
    public int IdPago { get; init; }
    public int IdVenta { get; init; }
    public DateTime FechaPago { get; init; }
    public decimal Monto { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public string? Observacion { get; init; }
    public int IdUsuario { get; init; }
    public string Usuario { get; init; } = string.Empty;
    public int IdSesionCaja { get; init; }
}
