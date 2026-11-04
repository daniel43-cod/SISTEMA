namespace API_SISTEMA.Dtos.Compras;

public sealed class ReporteComprasDto
{
    public int IdSesionCaja { get; init; }
    public DateTime Apertura { get; init; }
    public DateTime Hasta { get; init; }
    public decimal TotalCompras { get; init; }
    public int CantidadCompras { get; init; }
    public List<PuntoCompraDto> Puntos { get; init; } = [];
}

public sealed class PuntoCompraDto
{
    public int IdCompra { get; init; }
    public DateTime Fecha { get; init; }
    public decimal MontoCompra { get; init; }
}