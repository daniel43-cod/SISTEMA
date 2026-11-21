using System.Net;
using System.Text;
using System.Xml.Linq;
using API_SISTEMA.Services.Fel;

internal static class CertificadoFelChecks
{
    public static void Run(string enviado)
    {
        XNamespace ns = "http://www.sat.gob.gt/dte/fel/0.2.0";
        var uuid = Guid.NewGuid().ToString();
        var certificado = XDocument.Parse(enviado);
        var dte = certificado.Descendants(ns + "DTE").Single();
        dte.Add(new XElement(ns + "Certificacion",
            new XElement(ns + "NITCertificador", "1234567"),
            new XElement(ns + "NombreCertificador", "CERTIFICADOR DE PRUEBA"),
            new XElement(ns + "NumeroAutorizacion", new XAttribute("Serie", "**PRUEBAS**"), new XAttribute("Numero", "2959820099"), uuid),
            new XElement(ns + "FechaHoraCertificacion", "2026-11-19T12:00:00-06:00")));
        InfileRespuesta Respuesta(string xml) => new(HttpStatusCode.OK, "{}", true, uuid, "**PRUEBAS**", "2959820099",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(xml)));
        var respuesta = Respuesta(certificado.ToString());
        var checks = 0;
        void Check(bool condicion, string caso) { if (!condicion) throw new Exception("Certificado FEL: " + caso); checks++; }
        void Rechazar(XDocument alterado, string caso)
        {
            var resultado = ValidarCertificadoFel.Evaluar(enviado, Respuesta(alterado.ToString()));
            Check(!resultado.Valido && resultado.Fecha is null && resultado.Error is not null && resultado.Xml is not null, caso);
        }
        var valido = ValidarCertificadoFel.Evaluar(enviado, respuesta);
        Check(valido.Valido && valido.Xml is not null && valido.Fecha!.Value.Offset == TimeSpan.FromHours(-6), "documento correcto");

        // Cada atributo y cada valor de emisión se modifica por separado: incluye
        // NIT, CUI/CF, nombres, moneda, tipo, líneas, impuestos y totales.
        var datos = certificado.Descendants(ns + "DatosEmision").Single();
        var atributos = datos.DescendantsAndSelf().Attributes().Where(a => !a.IsNamespaceDeclaration).ToArray();
        for (var i = 0; i < atributos.Length; i++)
        {
            var copia = new XDocument(certificado);
            copia.Descendants(ns + "DatosEmision").Single().DescendantsAndSelf().Attributes()
                .Where(a => !a.IsNamespaceDeclaration).ElementAt(i).Value += "ALTERADO";
            Rechazar(copia, "atributo alterado " + atributos[i].Name);
        }
        var hojas = datos.DescendantsAndSelf().Where(e => !e.HasElements).ToArray();
        for (var i = 0; i < hojas.Length; i++)
        {
            var copia = new XDocument(certificado);
            copia.Descendants(ns + "DatosEmision").Single().DescendantsAndSelf().Where(e => !e.HasElements)
                .ElementAt(i).Value += "ALTERADO";
            Rechazar(copia, "valor alterado " + hojas[i].Name);
        }
        foreach (var nombre in new[] { "Emisor", "Receptor", "DatosGenerales", "Items", "Totales" })
        {
            var copia = new XDocument(certificado);
            copia.Descendants(ns + nombre).Single().Remove();
            Rechazar(copia, "bloque faltante " + nombre);
        }
        foreach (var nombre in new[] { "DatosEmision", "Certificacion", "NumeroAutorizacion", "FechaHoraCertificacion" })
        {
            var copia = new XDocument(certificado);
            var elemento = copia.Descendants(ns + nombre).Single();
            elemento.AddAfterSelf(new XElement(elemento));
            Rechazar(copia, "elemento duplicado " + nombre);
        }
        foreach (var atributo in new[] { "Serie", "Numero" })
        {
            var copia = new XDocument(certificado);
            copia.Descendants(ns + "NumeroAutorizacion").Single().Attribute(atributo)!.Value = "OTRO";
            Rechazar(copia, "metadato diferente " + atributo);
        }
        foreach (var nombre in new[] { "NumeroAutorizacion", "FechaHoraCertificacion" })
        {
            var copia = new XDocument(certificado);
            copia.Descendants(ns + nombre).Single().Value = "INVALIDO";
            Rechazar(copia, nombre + " inválido");
            copia.Descendants(ns + nombre).Single().Remove();
            Rechazar(copia, nombre + " faltante");
        }
        var equivalente = new XDocument(certificado);
        foreach (var elemento in equivalente.Descendants())
        {
            var attrs = elemento.Attributes().Where(a => !a.IsNamespaceDeclaration).Select(a => new XAttribute(a)).Reverse().ToArray();
            elemento.Attributes().Where(a => !a.IsNamespaceDeclaration).Remove();
            elemento.Add(attrs);
        }
        foreach (var nombre in new[] { "Cantidad", "PrecioUnitario", "Precio", "Descuento", "MontoGravable", "MontoImpuesto", "Total", "GranTotal" })
            foreach (var elemento in equivalente.Descendants(ns + nombre))
                elemento.Value = decimal.Parse(elemento.Value, System.Globalization.CultureInfo.InvariantCulture)
                    .ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);
        Check(ValidarCertificadoFel.Evaluar(enviado, Respuesta(equivalente.ToString())).Valido, "orden de atributos y escala decimal equivalentes");
        var prefijos = certificado.ToString().Replace("xmlns:dte=", "xmlns:fel=").Replace("dte:", "fel:");
        Check(ValidarCertificadoFel.Evaluar(enviado, Respuesta(prefijos)).Valido, "prefijos equivalentes");
        Check(ValidarCertificadoFel.Evaluar(enviado, respuesta with { Uuid = uuid.ToUpperInvariant() }).Valido, "UUID sin distinción de mayúsculas");
        Check(ValidarCertificadoFel.Evaluar(enviado, Respuesta("\uFEFF" + certificado)).Valido, "UTF-8 con BOM");
        var importeDistinto = new XDocument(certificado);
        importeDistinto.Descendants(ns + "GranTotal").Single().Value = "112.000000000000000000000000000000001";
        Rechazar(importeDistinto, "diferencia numérica sin redondeo implícito");
        var agregado = new XDocument(certificado);
        agregado.Descendants(ns + "Items").Single().Add(new XElement(agregado.Descendants(ns + "Item").First()));
        Rechazar(agregado, "línea adicional");
        foreach (var tipo in new[] { "CF", "CUI", "NIT" })
        {
            var originalReceptor = XDocument.Parse(enviado);
            var receptor = originalReceptor.Descendants(ns + "Receptor").Single();
            receptor.SetAttributeValue("IDReceptor", tipo == "CF" ? "CF" : tipo == "CUI" ? "2993930620101" : "5203333");
            receptor.SetAttributeValue("TipoEspecial", tipo == "CUI" ? "CUI" : null);
            var certReceptor = new XDocument(originalReceptor);
            certReceptor.Descendants(ns + "DTE").Single().Add(new XElement(certificado.Descendants(ns + "Certificacion").Single()));
            Check(ValidarCertificadoFel.Evaluar(originalReceptor.ToString(), Respuesta(certReceptor.ToString())).Valido, "receptor " + tipo);
            certReceptor.Descendants(ns + "Receptor").Single().SetAttributeValue("IDReceptor", "OTRO");
            Check(!ValidarCertificadoFel.Evaluar(originalReceptor.ToString(), Respuesta(certReceptor.ToString())).Valido, "identidad alterada " + tipo);
        }
        foreach (var invalida in new[]
        {
            respuesta with { Uuid = Guid.NewGuid().ToString() }, respuesta with { Uuid = Guid.Empty.ToString() },
            respuesta with { Serie = "OTRA" }, respuesta with { Numero = "123" }, respuesta with { Resultado = false },
            respuesta with { EstadoHttp = HttpStatusCode.BadGateway }, respuesta with { XmlCertificadoBase64 = null },
            respuesta with { XmlCertificadoBase64 = "no base64!" }, Respuesta("<xml>"), Respuesta("<otro />"),
            Respuesta("<!DOCTYPE x [<!ENTITY e SYSTEM 'file:///no-leer'>]><x>&e;</x>"),
            respuesta with { XmlCertificadoBase64 = Convert.ToBase64String(new byte[] { 0xff, 0xfe }) }
        }) Check(!ValidarCertificadoFel.Evaluar(enviado, invalida).Valido, "respuesta inválida o XML inseguro");
        var anidado = new XDocument(certificado);
        var cert = anidado.Descendants(ns + "Certificacion").Single();
        cert.Remove(); anidado.Root!.Add(cert);
        Rechazar(anidado, "certificación fuera del DTE");
        Check(!ValidarCertificadoFel.Evaluar("<incompleto>", respuesta).Valido, "original malformado");
        Console.WriteLine($"Certificado FEL: {checks} comprobaciones de correspondencia y XML seguro aprobadas.");
    }
}
