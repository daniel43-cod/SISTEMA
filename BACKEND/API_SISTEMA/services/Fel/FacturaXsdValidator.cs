using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace API_SISTEMA.Services.Fel;

public sealed class FacturaXsdValidator
{
    private readonly XmlSchemaSet schemas;

    public FacturaXsdValidator(IWebHostEnvironment environment)
        : this(Path.Combine(environment.ContentRootPath, "Schemas", "Fel")) { }

    public FacturaXsdValidator(string carpeta)
    {
        // No se resuelven URLs ni DTD externos al cargar esquemas o facturas.
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
        using var firmaReader = XmlReader.Create(Path.Combine(carpeta, "xmldsig-core-schema.xsd"), settings);
        var firma = XmlSchema.Read(firmaReader, null)!;
        using var satReader = XmlReader.Create(Path.Combine(carpeta, "GT_Documento-0.2.1.xsd"), settings);
        var original = XDocument.Load(satReader);
        XNamespace xs = XmlSchema.Namespace;
        // Proyección en memoria para PRE-FIRMA. El XSD oficial en disco no se modifica.
        original.Descendants(xs + "element")
            .Where(e => (string?)e.Attribute("id") is "FirmaEmisor" or "FirmaCertificador")
            .Remove();
        using var preFirmaReader = original.CreateReader();
        var preFirma = XmlSchema.Read(preFirmaReader, null)!;
        foreach (XmlSchemaImport import in preFirma.Includes.OfType<XmlSchemaImport>())
        {
            import.SchemaLocation = null;
            if (import.Namespace == "http://www.w3.org/2000/09/xmldsig#") import.Schema = firma;
        }
        schemas = new XmlSchemaSet { XmlResolver = null };
        schemas.Add(preFirma);
        schemas.Compile();
    }

    public void ValidarAntesDeFirma(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 10_000_000
        });
        try
        {
            var document = XDocument.Load(reader);
            var errores = new List<string>();
            document.Validate(schemas, (_, e) =>
            {
                if (errores.Count < 10) errores.Add(e.Message);
            });
            if (errores.Count > 0) throw new ValidationException("XML FEL inválido: " + string.Join(" | ", errores));
        }
        catch (XmlException ex)
        {
            throw new ValidationException("El XML FEL no está bien formado.", ex);
        }
    }
}
