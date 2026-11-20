using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Xml.Linq;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Ventas;

namespace API_SISTEMA.Services.Fel;

public sealed record DireccionFel(string Direccion, string CodigoPostal, string Municipio, string Departamento, string Pais);
public sealed record ReceptorFel(string Identificacion, string Nombre, DireccionFel? Direccion, string? Correo = null,
    string TipoIdentificacion = "NIT");
public sealed record LineaFacturaFel(string Descripcion, int Cantidad, decimal PrecioUnitario, decimal Descuento,
    string UnidadMedida, string BienOServicio, decimal? Impuesto = null);
public sealed record FraseFel(int Tipo, int Escenario);

// Primera versión: FACT en GTQ, régimen GEN, líneas afectas al IVA incluido del 12%.
// Las frases se reciben explícitamente; no se copian las del emisor de pruebas a producción.
public static class FacturaXmlGenerator
{
    private static readonly XNamespace Dte = "http://www.sat.gob.gt/dte/fel/0.2.0";
    private static string Texto(string? valor, string campo) => string.IsNullOrWhiteSpace(valor)
        ? throw new ValidationException($"Falta {campo} para emitir la factura.") : valor.Trim();
    private static string Numero(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);
    private static string NumeroImpuesto(decimal valor) => valor.ToString("0.00####", CultureInfo.InvariantCulture);
    private static decimal Redondear(decimal valor) => decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    private static XElement Direccion(string nombre, DireccionFel d) => new(Dte + nombre,
        new XElement(Dte + "Direccion", Texto(d.Direccion, "dirección")),
        new XElement(Dte + "CodigoPostal", Texto(d.CodigoPostal, "código postal")),
        new XElement(Dte + "Municipio", Texto(d.Municipio, "municipio")),
        new XElement(Dte + "Departamento", Texto(d.Departamento, "departamento")),
        new XElement(Dte + "Pais", Texto(d.Pais, "país").ToUpperInvariant()));

    public static string Generar(Empresa emisor, ReceptorFel receptor, DateTimeOffset fecha,
        IReadOnlyList<LineaFacturaFel> lineas, IReadOnlyList<FraseFel> frases, decimal totalEsperado, decimal? impuestoEsperado = null)
    {
        if (receptor.TipoIdentificacion is not ("NIT" or "CUI"))
            throw new ValidationException("El tipo de identificación debe ser NIT o CUI.");
        var identidad = IdentificacionReceptorFel.Resolver(
            receptor.TipoIdentificacion == "NIT" ? receptor.Identificacion : null,
            receptor.TipoIdentificacion == "CUI" ? receptor.Identificacion : null, totalEsperado);
        if (emisor.afiliacion_iva?.Trim().ToUpperInvariant() != "GEN")
            throw new ValidationException("Esta versión de FACT requiere afiliación IVA GEN.");
        if (emisor.codigo_establecimiento is null or <= 0)
            throw new ValidationException("Falta el código de establecimiento autorizado.");
        if (lineas.Count == 0 || frases.Count == 0 || frases.Any(f => f.Tipo <= 0 || f.Escenario <= 0))
            throw new ValidationException("La factura requiere líneas y frases fiscales válidas.");
        var items = new XElement(Dte + "Items");
        decimal total = 0, iva = 0;
        foreach (var linea in lineas)
        {
            if (linea.Cantidad <= 0 || linea.PrecioUnitario <= 0 || linea.Descuento < 0 ||
                Redondear(linea.PrecioUnitario) != linea.PrecioUnitario || Redondear(linea.Descuento) != linea.Descuento ||
                linea.BienOServicio is not ("B" or "S"))
                throw new ValidationException("Revisa cantidades, precios, descuentos y clasificación de las líneas FEL.");
            var bruto = linea.Cantidad * linea.PrecioUnitario;
            var neto = bruto - linea.Descuento;
            if (neto <= 0) throw new ValidationException("El total de cada línea debe ser mayor a cero.");
            var calculoIva = IvaIncluido.Calcular(neto);
            if (linea.Impuesto.HasValue && linea.Impuesto.Value != calculoIva.Impuesto)
                throw new ValidationException("El IVA guardado de una línea no coincide con su importe. Revisa la venta antes de certificar.");
            var impuesto = linea.Impuesto ?? calculoIva.Impuesto;
            var baseIva = neto - impuesto;
            total += neto;
            iva += impuesto;
            items.Add(new XElement(Dte + "Item", new XAttribute("BienOServicio", linea.BienOServicio),
                new XAttribute("NumeroLinea", items.Elements().Count() + 1),
                new XElement(Dte + "Cantidad", linea.Cantidad),
                new XElement(Dte + "UnidadMedida", Texto(linea.UnidadMedida, "unidad de medida")),
                new XElement(Dte + "Descripcion", Texto(linea.Descripcion, "descripción del producto")),
                new XElement(Dte + "PrecioUnitario", Numero(linea.PrecioUnitario)),
                new XElement(Dte + "Precio", Numero(bruto)),
                new XElement(Dte + "Descuento", Numero(linea.Descuento)),
                new XElement(Dte + "Impuestos", new XElement(Dte + "Impuesto",
                    new XElement(Dte + "NombreCorto", "IVA"), new XElement(Dte + "CodigoUnidadGravable", 1),
                    new XElement(Dte + "MontoGravable", NumeroImpuesto(baseIva)),
                    new XElement(Dte + "MontoImpuesto", NumeroImpuesto(impuesto)))),
                new XElement(Dte + "Total", Numero(neto))));
        }
        if (total != totalEsperado) throw new ValidationException("Las líneas fiscales no coinciden con el total de la venta.");
        if (impuestoEsperado.HasValue && impuestoEsperado.Value != iva)
            throw new ValidationException("El IVA total guardado no coincide con las líneas de la venta.");
        var emision = new XElement(Dte + "DatosEmision", new XAttribute("ID", "DatosEmision"),
            new XElement(Dte + "DatosGenerales", new XAttribute("CodigoMoneda", "GTQ"),
                new XAttribute("FechaHoraEmision", fecha.ToOffset(TimeSpan.FromHours(-6)).ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture)),
                new XAttribute("Tipo", "FACT")),
            new XElement(Dte + "Emisor", new XAttribute("AfiliacionIVA", "GEN"),
                new XAttribute("CodigoEstablecimiento", emisor.codigo_establecimiento.Value),
                new XAttribute("CorreoEmisor", Texto(emisor.correo_emisor, "correo del emisor")),
                new XAttribute("NITEmisor", Texto(emisor.nit, "NIT del emisor")),
                new XAttribute("NombreComercial", Texto(emisor.nombre_comercial, "nombre comercial")),
                new XAttribute("NombreEmisor", Texto(emisor.razon_social, "razón social")),
                Direccion("DireccionEmisor", new(emisor.direccion!, emisor.codigo_postal!, emisor.municipio!, emisor.departamento!, emisor.pais!))),
            new XElement(Dte + "Receptor", new XAttribute("IDReceptor", identidad.Identificacion),
                identidad.Tipo == "CUI" ? new XAttribute("TipoEspecial", "CUI") : null,
                new XAttribute("NombreReceptor", Texto(receptor.Nombre, "nombre del receptor")),
                string.IsNullOrWhiteSpace(receptor.Correo) ? null : new XAttribute("CorreoReceptor", receptor.Correo.Trim()),
                identidad.Identificacion == "CF" || receptor.Direccion is null ? null : Direccion("DireccionReceptor", receptor.Direccion)),
            new XElement(Dte + "Frases", frases.Select(f => new XElement(Dte + "Frase",
                new XAttribute("TipoFrase", f.Tipo), new XAttribute("CodigoEscenario", f.Escenario)))),
            items, new XElement(Dte + "Totales", new XElement(Dte + "TotalImpuestos",
                new XElement(Dte + "TotalImpuesto", new XAttribute("NombreCorto", "IVA"),
                    new XAttribute("TotalMontoImpuesto", NumeroImpuesto(iva)))), new XElement(Dte + "GranTotal", Numero(total))));
        var raiz = new XElement(Dte + "GTDocumento", new XAttribute(XNamespace.Xmlns + "dte", Dte.NamespaceName),
            new XAttribute("Version", "0.1"), new XElement(Dte + "SAT", new XAttribute("ClaseDocumento", "dte"),
                new XElement(Dte + "DTE", new XAttribute("ID", "DatosCertificados"), emision)));
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + raiz.ToString(SaveOptions.DisableFormatting);
    }
}
