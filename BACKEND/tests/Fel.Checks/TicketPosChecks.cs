using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Comprobantes;
using API_SISTEMA.Services.Fel;

internal static class TicketPosChecks
{
    public static void Run(Empresa empresa)
    {
        var checks = 0;
        void Check(bool condicion, string nombre) { if (!condicion) throw new Exception("Ticket POS: " + nombre); checks++; }
        void Rechazar(Action accion)
        {
            try { accion(); }
            catch (ValidationException) { checks++; return; }
            throw new Exception("Se aceptó un ticket inválido.");
        }
        var venta = new API_SISTEMA.Models.Ventas
        {
            id_ventas = 22, origen = "CAJA", fecha_venta = new DateTime(2026, 11, 21, 12, 0, 0), total = 112m, impuesto = 12m,
            usuario = new Usuario { nombre = "VENDEDOR", apellido = "PRUEBA", usuario = "demo", telefono = "123" },
            cliente = new Cliente { nombre = "CLIENTE INTERNO", apellido = "DEMO" },
            DetalleVentas = [new DetalleVenta { cantidad = 2, precio = 60m, descuento = 8m, subtotal = 112m, impuesto = 12m, Producto = new Productos { Nombre = "Producto interno" } }]
        };
        var interno = TicketPos.DesdeVenta(venta, empresa);
        Check(interno.Tipo == "COMPROBANTE_INTERNO" && interno.Certificacion is null && interno.Ambiente is null && interno.Avisos.Single().Contains("NO ES FACTURA FEL"), "No presentar comprobante interno como factura.");
        Check(interno.Subtotal == 120m && interno.Descuento == 8m && interno.Total == 112m && interno.Iva == 12m, "IVA incluido, sin sumarlo nuevamente.");
        venta.DetalleVentas[0].impuesto = null;
        Check(TicketPos.DesdeVenta(venta, empresa).Iva is null, "No inventar IVA histórico desconocido.");
        venta.total = 113m;
        Rechazar(() => TicketPos.DesdeVenta(venta, empresa));
        venta.total = 112m;
        XNamespace ns = "http://www.sat.gob.gt/dte/fel/0.2.0";
        foreach (var identificacion in new[] { "CF", "5203333", "2993930620101" })
        {
            var receptor = new ReceptorFel(identificacion, "RECEPTOR FISCAL", null, TipoIdentificacion: identificacion.Length == 13 ? "CUI" : "NIT");
            var enviado = FacturaXmlGenerator.Generar(empresa, receptor, DateTimeOffset.Parse("2026-11-21T12:00:00-06:00"),
                [new("Producto certificado", 2, 60m, 8m, "UND", "B")], [new(1, 1), new(2, 1)], 112m);
            var xml = XDocument.Parse(enviado);
            var uuid = Guid.NewGuid().ToString();
            xml.Descendants(ns + "DTE").Single().Add(new XElement(ns + "Certificacion",
                new XElement(ns + "NITCertificador", "1234567"), new XElement(ns + "NombreCertificador", "CERTIFICADOR DEMO"),
                new XElement(ns + "NumeroAutorizacion", new XAttribute("Serie", "**PRUEBAS**"), new XAttribute("Numero", "2959820099"), uuid),
                new XElement(ns + "FechaHoraCertificacion", "2026-11-21T12:00:01-06:00")));
            var d = new DocumentoFel { id_venta = 22, estado = "CERTIFICADA", ambiente = "PRUEBAS", uuid = uuid, serie = "**PRUEBAS**", numero = "2959820099", xml_enviado = enviado, xml_certificado = xml.ToString() };
            var ticket = TicketPos.DesdeCertificado(d, "VENDEDOR");
            Check(ticket.Receptor.Nombre == "RECEPTOR FISCAL" && ticket.Receptor.Identificacion == identificacion && ticket.Lineas.Single().Descripcion == "Producto certificado", "Usar snapshot fiscal, no nombres internos.");
            Check(ticket.Certificacion?.Uuid == uuid && ticket.Certificacion.NombreCertificador == "CERTIFICADOR DEMO" && ticket.Total == 112m && ticket.Subtotal == 120m, "Metadatos y total del DTE.");
            Check(ticket.Certificacion!.QrUrl.Contains("serie=%2A%2APRUEBAS%2A%2A") && ticket.Certificacion.QrUrl.Contains("numero=2959820099") && ticket.Certificacion.QrUrl.Contains("emisor=" + empresa.nit), "QR SAT con parámetros escapados.");
            Check(ticket.Frases.SequenceEqual(new[] { "Sujeto a pagos trimestrales ISR", "Agente de Retención del IVA" }) && ticket.Avisos.Single().Contains("SIN VALIDEZ FISCAL"), "Frases fiscales y pruebas visibles.");
            d.ambiente = "PRODUCCION";
            Check(TicketPos.DesdeCertificado(d, "VENDEDOR").Avisos.Count == 0, "No introducir leyenda de pruebas en producción.");
            d.ambiente = "PRUEBAS";
            var original = d.xml_certificado;
            d.xml_certificado = d.xml_certificado.Replace("Producto certificado", "ALTERADO");
            Rechazar(() => TicketPos.DesdeCertificado(d, "VENDEDOR"));
            d.xml_certificado = original;
            d.serie = "OTRA";
            Rechazar(() => TicketPos.DesdeCertificado(d, "VENDEDOR"));
            d.serie = "**PRUEBAS**";
            d.estado = "POR_CONFIRMAR";
            Rechazar(() => TicketPos.DesdeCertificado(d, "VENDEDOR"));
            d.estado = "CERTIFICADA";
            // Un escenario sin texto admitido no debe imprimirse como representación fiscal incompleta.
            d.xml_certificado = original.Replace("TipoFrase=\"2\"", "TipoFrase=\"9\"");
            d.xml_enviado = enviado.Replace("TipoFrase=\"2\"", "TipoFrase=\"9\"");
            Rechazar(() => TicketPos.DesdeCertificado(d, "VENDEDOR"));
        }
        var atributos = typeof(API_SISTEMA.Controllers.TicketPosController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        Check(atributos.Length == 1, "Endpoint protegido por autorización.");
        Console.WriteLine($"Ticket POS: {checks} comprobaciones backend aprobadas, sin impresora ni llamadas reales.");
    }
}
