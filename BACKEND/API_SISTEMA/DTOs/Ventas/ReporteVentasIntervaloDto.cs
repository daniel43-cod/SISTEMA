using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Ventas;

public sealed class FiltroReporteVentasIntervaloDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int IdSesionCaja { get; init; }
    public int IntervaloMinutos { get; init; } = 30;
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IntervaloMinutos is not (30 or 60))
            yield return new ValidationResult("El intervalo debe ser de 30 o 60 minutos.", [nameof(IntervaloMinutos)]);
    }
}

public sealed class ReporteVentasIntervaloDto
{
    public int IdSesionCaja { get; init; }
    public DateTime Apertura { get; init; }
    public DateTime? Cierre { get; init; }
    public DateTime Hasta { get; init; }
    public int IntervaloMinutos { get; init; }
    public decimal TotalVentas { get; init; }
    public int CantidadVentas { get; init; }
    public List<PuntoVentasIntervaloDto> Puntos { get; init; } = [];
}

public sealed class PuntoVentasIntervaloDto
{
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public decimal TotalVentas { get; set; }
    public int CantidadVentas { get; set; }
}
