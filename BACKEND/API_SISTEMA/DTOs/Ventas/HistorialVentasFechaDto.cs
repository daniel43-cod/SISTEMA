using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Ventas;

public sealed class HistorialVentasFechaDto
{
    public DateOnly? Desde { get; init; }
    public DateOnly? Hasta { get; init; }
    [Range(1, int.MaxValue)] public int? IdUsuario { get; init; }
    [Range(1, 100000)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; init; } = 50;
}
