namespace API_SISTEMA.Dtos.Ventas;

public sealed class ReporteVentasAcumuladasDto
{
    public int IdSesionCaja { get; init; }
    public DateTime Apertura { get; init; }
    public DateTime? Cierre { get; init; }
    public DateTime Hasta { get; init; }
    public decimal TotalVentas { get; init; }
    public int CantidadVentas { get; init; }
    public List<PuntoVentaAcumuladaDto> Puntos { get; init; } = [];
}

public sealed class PuntoVentaAcumuladaDto
{
    public int? IdVenta { get; init; }
    public DateTime Fecha { get; init; }
    public decimal MontoVenta { get; init; }
    public decimal GananciaVenta { get; init; }
    public decimal Acumulado { get; init; }
}
