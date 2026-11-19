using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Services.Fel;

namespace API_SISTEMA.Services.Ventas;

public sealed record DatosReceptorVenta(string Tipo, string Identificacion, string Nombre);

public static class ReceptorVentaReglas
{
    public static DatosReceptorVenta Validar(string? tipo, string? identificacion, string? nombre, decimal total)
    {
        tipo = tipo?.Trim().ToUpperInvariant();
        if (tipo is not ("CF" or "NIT" or "CUI"))
            throw new ValidationException("Selecciona el tipo de identificación del receptor: CF, NIT o CUI.");
        if (tipo == "CF")
        {
            if (!string.IsNullOrWhiteSpace(identificacion) && !string.Equals(identificacion.Trim(), "CF", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Consumidor final no debe incluir un NIT o CUI.");
            IdentificacionReceptorFel.Resolver("CF", null, total);
            return new("CF", "CF", "CONSUMIDOR FINAL");
        }
        if (string.IsNullOrWhiteSpace(identificacion) || string.Equals(identificacion.Trim(), "CF", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Proporciona el documento correspondiente al tipo NIT o CUI seleccionado.");
        var identidad = IdentificacionReceptorFel.Resolver(tipo == "NIT" ? identificacion : null,
            tipo == "CUI" ? identificacion : null, total);
        VentaReglas.Texto(nombre, 250, "el nombre fiscal del receptor", true);
        return new(tipo, identidad.Identificacion, nombre!.Trim());
    }
}
