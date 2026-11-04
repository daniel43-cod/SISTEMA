namespace API_SISTEMA.Dtos.Gastos;

public sealed class ReporteGastosDto
{
    public int IdSesionCaja { get; init; }
    public DateTime Apertura { get; init; }
    public DateTime Hasta { get; init; }
    public decimal TotalGastos { get; init; }
    public int CantidadGastos { get; init; }
    public List<PuntoGastoDto> Puntos { get; init; } = [];
}

public sealed class PuntoGastoDto
{
    public int IdGasto { get; init; }
    public DateTime Fecha { get; init; }
    public decimal MontoGasto { get; init; }
}