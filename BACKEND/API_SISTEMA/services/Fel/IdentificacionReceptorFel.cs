using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace API_SISTEMA.Services.Fel;

public static class IdentificacionReceptorFel
{
    public static (string Identificacion, string Tipo) Resolver(string? nit, string? cui, decimal total)
    {
        var tieneNit = !string.IsNullOrWhiteSpace(nit);
        var tieneCui = !string.IsNullOrWhiteSpace(cui);
        if (tieneNit && tieneCui)
            throw new ValidationException("Envía NIT o CUI del receptor, no ambos.");
        if (tieneCui)
        {
            var valor = cui!.Trim();
            if (!Regex.IsMatch(valor, "^[0-9]{13}$"))
                throw new ValidationException("El CUI del receptor debe contener exactamente 13 dígitos.");
            return (valor, "CUI");
        }
        var identificacion = nit?.Trim().Replace("-", "").ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(identificacion) || identificacion == "CF")
        {
            if (total >= 2500m)
                throw new ValidationException("Para facturas de Q2,500.00 o más debes proporcionar NIT o CUI del receptor. No se permite consumidor final (CF).");
            if (string.IsNullOrWhiteSpace(identificacion))
                throw new ValidationException("Indica NIT, CUI o CF para identificar al receptor.");
            return ("CF", "NIT");
        }
        if (!Regex.IsMatch(identificacion, "^[0-9]+[0-9K]$"))
            throw new ValidationException("El NIT del receptor tiene un formato inválido.");
        return (identificacion, "NIT");
    }
}
