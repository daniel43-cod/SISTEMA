using System.ComponentModel.DataAnnotations;
namespace API_SISTEMA.Dtos.Compras;
public sealed class FiltroResumenComprasDto
{
    [Range(1, 100000)] public int Pagina { get; set; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; set; } = 20;
    [StringLength(100)] public string? Buscar { get; set; }
    [RegularExpression("^(pendientes|pagadas)$")] public string? EstadoPago { get; set; }
}
public sealed class ResumenComprasDto
{
    public int TotalRegistros { get; init; }
    public int CantidadPendientes { get; init; }
    public int CantidadPagadas { get; init; }
    public decimal TotalComprado { get; init; }
    public decimal TotalPorPagar { get; init; }
    public decimal TotalPagado { get; init; }
    public bool HayMas { get; init; }
    public List<ListarComprasDto> Items { get; init; } = [];
}