using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Fel;

namespace API_SISTEMA.Services.Comprobantes;

public sealed record PersonaTicket(string Nombre, string? RazonSocial, string? Identificacion, string? Direccion);
public sealed record LineaTicket(string Descripcion, decimal Cantidad, string Unidad, decimal PrecioUnitario, decimal Descuento, decimal Total);
public sealed record CertificacionTicket(string Uuid, string Serie, string Numero, string Fecha, string NombreCertificador, string NitCertificador, string QrUrl);
public sealed record TicketPosDto(int IdVenta, string Tipo, string? Ambiente, string Fecha, string Vendedor,
    PersonaTicket Emisor, PersonaTicket Receptor, IReadOnlyList<LineaTicket> Lineas,
    decimal Subtotal, decimal Descuento, decimal? Iva, decimal Total,
    CertificacionTicket? Certificacion, IReadOnlyList<string> Frases, IReadOnlyList<string> Avisos);

public static class TicketPos
{
    private static readonly XNamespace Ns = "http://www.sat.gob.gt/dte/fel/0.2.0";
    private static ValidationException Invalido() => new("El DTE guardado no permite generar un ticket íntegro. Utiliza el PDF certificado de INFILE y revisa el documento.");
    private static string A(XElement e, string nombre) => (string?)e.Attribute(nombre) is { Length: > 0 } valor ? valor : throw Invalido();
    private static string V(XElement e, string nombre) => e.Element(Ns + nombre)?.Value is { Length: > 0 } valor ? valor : throw Invalido();
    private static XElement E(XElement e, string nombre) => e.Element(Ns + nombre) ?? throw Invalido();
    private static decimal N(string valor) => decimal.TryParse(valor, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var numero) && numero >= 0 ? numero : throw Invalido();
    private static string? Direccion(XElement? e) => e is null ? null : string.Join(", ", e.Elements().Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)));

    public static TicketPosDto DesdeCertificado(DocumentoFel d, string vendedor)
    {
        try
        {
            if (d.estado != "CERTIFICADA" || string.IsNullOrWhiteSpace(d.xml_certificado)) throw Invalido();
            // Incluso documentos anteriores a esta versión deben corresponder al XML emitido.
            var evaluacion = ValidarCertificadoFel.Evaluar(d.xml_enviado, new(HttpStatusCode.OK, "{}", true,
                d.uuid, d.serie, d.numero, Convert.ToBase64String(Encoding.UTF8.GetBytes(d.xml_certificado))));
            if (!evaluacion.Valido) throw Invalido();
            using var lector = XmlReader.Create(new StringReader(d.xml_certificado.TrimStart('\uFEFF')), new XmlReaderSettings
            { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 10_000_000 });
            var xml = XDocument.Load(lector);
            var documento = E(E(xml.Root!, "SAT"), "DTE");
            var datos = E(documento, "DatosEmision");
            var generales = E(datos, "DatosGenerales");
            if (A(generales, "Tipo") != "FACT" || A(generales, "CodigoMoneda") != "GTQ") throw Invalido();
            var emisor = E(datos, "Emisor"); var receptor = E(datos, "Receptor");
            var certificacion = E(documento, "Certificacion");
            var lineas = E(datos, "Items").Elements(Ns + "Item").Select(i => new LineaTicket(V(i, "Descripcion"),
                N(V(i, "Cantidad")), V(i, "UnidadMedida"), N(V(i, "PrecioUnitario")), N(V(i, "Descuento")), N(V(i, "Total")))).ToArray();
            var descuento = lineas.Sum(i => i.Descuento);
            var total = N(V(E(datos, "Totales"), "GranTotal"));
            if (lineas.Length == 0 || lineas.Sum(i => i.Total) != total) throw Invalido();
            var iva = E(datos, "Totales").Element(Ns + "TotalImpuestos")?.Elements(Ns + "TotalImpuesto")
                .Where(i => (string?)i.Attribute("NombreCorto") == "IVA").Sum(i => N(A(i, "TotalMontoImpuesto")));
            var frases = datos.Element(Ns + "Frases")?.Elements(Ns + "Frase").Select(Frase).ToArray() ?? [];
            if (d.ambiente is not ("PRUEBAS" or "PRODUCCION")) throw Invalido();
            var qr = "https://portal.sat.gob.gt/portal/verificador-integrado/?tipo=autorizacion&documento=FEL&emisor=" +
                Uri.EscapeDataString(A(emisor, "NITEmisor")) + "&serie=" + Uri.EscapeDataString(d.serie!) + "&numero=" + Uri.EscapeDataString(d.numero!);
            return new(d.id_venta, "FACTURA_FEL", d.ambiente, A(generales, "FechaHoraEmision"), vendedor,
                new(A(emisor, "NombreComercial"), A(emisor, "NombreEmisor"), A(emisor, "NITEmisor"), Direccion(emisor.Element(Ns + "DireccionEmisor"))),
                new(A(receptor, "NombreReceptor"), null, A(receptor, "IDReceptor"), Direccion(receptor.Element(Ns + "DireccionReceptor"))),
                lineas, total + descuento, descuento, iva, total,
                new(d.uuid!, d.serie!, d.numero!, V(certificacion, "FechaHoraCertificacion"), V(certificacion, "NombreCertificador"), V(certificacion, "NITCertificador"), qr),
                frases, d.ambiente == "PRUEBAS" ? ["DOCUMENTO DE PRUEBAS - SIN VALIDEZ FISCAL"] : []);
        }
        catch (Exception ex) when (ex is XmlException or InvalidOperationException or OverflowException) { throw Invalido(); }
    }

    // Catálogo explícito para las frases del flujo FACT actual. No se inventa texto para otros escenarios.
    private static string Frase(XElement frase) => (A(frase, "TipoFrase"), A(frase, "CodigoEscenario")) switch
    {
        ("1", "1") => "Sujeto a pagos trimestrales ISR",
        ("1", "2") => "Sujeto a retención definitiva ISR",
        ("2", "1") => "Agente de Retención del IVA",
        ("3", "1") => "No genera derecho a crédito fiscal",
        _ => throw new ValidationException("El ticket no tiene configurado el texto de una frase fiscal de este DTE. Utiliza el PDF certificado de INFILE.")
    };

    public static TicketPosDto DesdeVenta(Models.Ventas venta, Empresa empresa)
    {
        var lineas = venta.DetalleVentas.OrderBy(d => d.id_detalle_venta).Select(d => new LineaTicket(
            string.Join(" · ", new[] { d.Producto.Nombre, d.producto_presentacion?.Presentacion?.Descripcion }.Where(s => !string.IsNullOrWhiteSpace(s))),
            d.cantidad, "UND", d.precio, d.descuento ?? 0m, d.subtotal)).ToArray();
        if (lineas.Length == 0 || lineas.Sum(i => i.Total) != venta.total) throw new ValidationException("El detalle de la venta no coincide con su total.");
        return new(venta.id_ventas, "COMPROBANTE_INTERNO", null, venta.fecha_venta.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
            (venta.usuario.nombre + " " + venta.usuario.apellido).Trim(),
            new(empresa.nombre_comercial ?? empresa.nombre_empresa, empresa.razon_social, empresa.nit, empresa.direccion ?? empresa.ubicacion),
            new((venta.cliente.nombre + " " + venta.cliente.apellido).Trim(), null, null, null),
            lineas, venta.total + lineas.Sum(i => i.Descuento), lineas.Sum(i => i.Descuento),
            venta.DetalleVentas.All(i => i.impuesto.HasValue) ? venta.impuesto : null, venta.total,
            null, [], ["COMPROBANTE INTERNO - NO ES FACTURA FEL"]);
    }
}
