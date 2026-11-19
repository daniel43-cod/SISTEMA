using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Services.Ventas;

public static class IvaIncluido
{
    // Solo para líneas gravadas al 12%, con el impuesto ya incluido en el importe final.
    public static (decimal Base, decimal Impuesto) Calcular(decimal importeFinal)
    {
        if (importeFinal < 0 || importeFinal > VentaReglas.MontoMaximo)
            throw new ValidationException("El importe para calcular IVA está fuera del rango permitido.");
        var baseGravable = decimal.Round(importeFinal / 1.12m, 6, MidpointRounding.AwayFromZero);
        return (baseGravable, decimal.Round(importeFinal - baseGravable, 6, MidpointRounding.AwayFromZero));
    }
}
