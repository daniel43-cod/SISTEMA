using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace API_SISTEMA.Services.Fel;

public sealed record ValidacionCertificadoFel(bool Valido, string? Xml, DateTimeOffset? Fecha, string? Error);

// Comprueba correspondencia del documento; no verifica criptográficamente la firma.
public static class ValidarCertificadoFel
{
    private static readonly XNamespace Dte = "http://www.sat.gob.gt/dte/fel/0.2.0";
    private const int MaximoXml = 10_000_000;
    private static readonly HashSet<string> Decimales =
        ["Cantidad", "PrecioUnitario", "Precio", "Descuento", "MontoGravable", "MontoImpuesto", "Total", "GranTotal"];

    public static ValidacionCertificadoFel Evaluar(string enviado, InfileRespuesta respuesta)
    {
        string? xml = null;
        ValidacionCertificadoFel Fallo() => new(false, xml, null,
            "No se pudo comprobar que el XML certificado corresponda al enviado. Consulta y recupera el documento con el mismo identificador.");
        try
        {
            if (respuesta.XmlCertificadoBase64 is not { Length: > 0 and <= 13_333_336 }) return Fallo();
            var bytes = Convert.FromBase64String(respuesta.XmlCertificadoBase64);
            if (bytes.Length > MaximoXml) return Fallo();
            xml = new UTF8Encoding(false, true).GetString(bytes);
            if ((int)respuesta.EstadoHttp is < 200 or >= 300 || respuesta.Resultado != true ||
                !Guid.TryParse(respuesta.Uuid, out var uuid) || uuid == Guid.Empty ||
                respuesta.Serie?.Length is not (>= 1 and <= 20) || respuesta.Numero?.Length is not (>= 1 and <= 30)) return Fallo();
            var original = Leer(enviado);
            var certificado = Leer(xml);
            var datosOriginales = Datos(original);
            var datosCertificados = Datos(certificado);
            if (datosOriginales is null || datosCertificados is null || !Iguales(datosOriginales, datosCertificados)) return Fallo();
            var dte = certificado.Root!.Element(Dte + "SAT")!.Element(Dte + "DTE")!;
            var certificacion = dte.Elements(Dte + "Certificacion").SingleOrDefault();
            if (certificacion is null || certificado.Descendants(Dte + "Certificacion").Count() != 1) return Fallo();
            var autorizacion = certificacion.Elements(Dte + "NumeroAutorizacion").SingleOrDefault();
            var fecha = certificacion.Elements(Dte + "FechaHoraCertificacion").SingleOrDefault()?.Value;
            if (!Guid.TryParse(autorizacion?.Value.Trim(), out var uuidXml) || uuidXml != uuid ||
                (string?)autorizacion!.Attribute("Serie") != respuesta.Serie ||
                (string?)autorizacion.Attribute("Numero") != respuesta.Numero ||
                !DateTimeOffset.TryParse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCertificada)) return Fallo();
            return new(true, xml, fechaCertificada, null);
        }
        catch (Exception ex) when (ex is FormatException or XmlException or InvalidOperationException or DecoderFallbackException)
        { return Fallo(); }
    }

    private static XDocument Leer(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml.TrimStart('\uFEFF')), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = MaximoXml,
            IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true
        });
        return XDocument.Load(reader);
    }

    private static XElement? Datos(XDocument xml)
    {
        if (xml.Root?.Name != Dte + "GTDocumento" || xml.Descendants(Dte + "SAT").Count() != 1 ||
            xml.Descendants(Dte + "DTE").Count() != 1 || xml.Descendants(Dte + "DatosEmision").Count() != 1) return null;
        return xml.Root.Element(Dte + "SAT")?.Element(Dte + "DTE")?.Element(Dte + "DatosEmision");
    }

    private static bool Iguales(XElement a, XElement b)
    {
        if (a.Name != b.Name) return false;
        var atributosA = a.Attributes().Where(x => !x.IsNamespaceDeclaration).OrderBy(x => x.Name.ToString()).ToArray();
        var atributosB = b.Attributes().Where(x => !x.IsNamespaceDeclaration).OrderBy(x => x.Name.ToString()).ToArray();
        if (atributosA.Length != atributosB.Length) return false;
        for (var i = 0; i < atributosA.Length; i++)
        {
            var x = atributosA[i]; var y = atributosB[i];
            if (x.Name != y.Name || !ValorIgual(x.Value, y.Value,
                a.Name == Dte + "TotalImpuesto" && x.Name == "TotalMontoImpuesto")) return false;
        }
        var hijosA = a.Elements().ToArray(); var hijosB = b.Elements().ToArray();
        if (hijosA.Length != hijosB.Length) return false;
        if (hijosA.Length == 0) return ValorIgual(a.Value, b.Value, a.Name.Namespace == Dte && Decimales.Contains(a.Name.LocalName));
        // Los contenedores FEL no admiten texto mezclado con sus elementos.
        if (a.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value)) ||
            b.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
        return hijosA.Zip(hijosB).All(par => Iguales(par.First, par.Second));
    }

    private static bool ValorIgual(string a, string b, bool numero)
        => numero ? Numero(a) is { } x && Numero(b) is { } y && x == y : a == b;

    // Comparación exacta sin redondear valores de más precisión que decimal.
    private static string? Numero(string valor)
    {
        var negativo = valor.StartsWith('-');
        if (valor.StartsWith('+') || negativo) valor = valor[1..];
        var partes = valor.Split('.');
        if (partes.Length > 2 || partes.Any(p => p.Length == 0 || p.Any(c => c is < '0' or > '9'))) return null;
        var entero = partes[0].TrimStart('0');
        var fraccion = partes.Length == 2 ? partes[1].TrimEnd('0') : "";
        return (negativo && (entero.Length != 0 || fraccion.Length != 0) ? "-" : "") +
            (entero.Length == 0 ? "0" : entero) + (fraccion.Length == 0 ? "" : "." + fraccion);
    }
}
