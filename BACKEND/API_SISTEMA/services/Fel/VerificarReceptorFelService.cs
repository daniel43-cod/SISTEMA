using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Utilidades;

namespace API_SISTEMA.Services.Fel;

public sealed class VerificarReceptorFelService(IConsultaReceptorFel consulta)
{
    public static (string Tipo, string Identificacion) Resolver(string? tipoPeticion, string? documentoPeticion,
        string? tipoGuardado, string? documentoGuardado, decimal total)
    {
        if (tipoPeticion is null && !string.IsNullOrWhiteSpace(documentoPeticion))
            throw new ValidationException("Indica el tipo de identificación al enviar un documento del receptor.");
        var tipo = (tipoPeticion ?? tipoGuardado)?.Trim().ToUpperInvariant();
        var documento = tipoPeticion is null ? documentoGuardado : documentoPeticion;
        if (documento?.Length > 20)
            throw new ValidationException("La identificación del receptor admite hasta 20 caracteres.");
        if (tipo is not ("CF" or "NIT" or "CUI"))
            throw new ValidationException("Selecciona CF, NIT o CUI antes de certificar.");
        if (tipo == "CF")
        {
            var cf = ReceptorVentaReglas.Validar(tipo, documento, null, total);
            return (cf.Tipo, cf.Identificacion);
        }
        if (string.IsNullOrWhiteSpace(documento) || string.Equals(documento.Trim(), "CF", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Proporciona el NIT o CUI correspondiente al receptor.");
        var normalizado = IdentificacionReceptorFel.Resolver(tipo == "NIT" ? documento : null, tipo == "CUI" ? documento : null, total);
        return (tipo, normalizado.Identificacion);
    }

    public async Task<DatosReceptorVenta> Verificar(string? tipoPeticion, string? documentoPeticion,
        string? tipoGuardado, string? documentoGuardado, decimal total, CancellationToken ct)
    {
        var receptor = Resolver(tipoPeticion, documentoPeticion, tipoGuardado, documentoGuardado, total);
        if (receptor.Tipo == "CF") return new("CF", "CF", "CONSUMIDOR FINAL");
        // Nunca se acepta como fuente el nombre que una API o cliente guardó manualmente.
        var respuesta = await consulta.Consultar(receptor.Tipo, receptor.Identificacion, ct);
        if (!respuesta.resultado || respuesta.tipo != receptor.Tipo || respuesta.identificacion != receptor.Identificacion ||
            (receptor.Tipo == "CUI" && respuesta.estatus != "ACTIVO"))
            throw new ValidationException("INFILE no pudo verificar el NIT o CUI del receptor. Consulta el documento antes de certificar.");
        return ReceptorVentaReglas.Validar(receptor.Tipo, receptor.Identificacion, respuesta.nombre, total);
    }

    public static void Confirmar(DatosReceptorVenta? verificado, string? tipoPeticion, string? documentoPeticion,
        string? tipoGuardado, string? documentoGuardado, decimal total)
    {
        var actual = Resolver(tipoPeticion, documentoPeticion, tipoGuardado, documentoGuardado, total);
        if (verificado is null || actual.Tipo != verificado.Tipo || actual.Identificacion != verificado.Identificacion)
            throw new VentaConflictoException("El receptor cambió durante la verificación. Consulta el estado y vuelve a certificar con los datos actuales.");
    }
}
