namespace API_SISTEMA.Models;

// Una factura FACT por venta y ambiente. Los reintentos conservan identificador y XML.
public sealed class DocumentoFel
{
    public int id_documento_fel { get; set; }
    public int id_venta { get; set; }
    public int id_usuario { get; set; }
    public string ambiente { get; set; } = "PRUEBAS";
    public string tipo_dte { get; set; } = "FACT";
    public string estado { get; set; } = "PENDIENTE";
    public Guid identificador { get; set; } = Guid.NewGuid();
    public string? uuid { get; set; }
    public string? serie { get; set; }
    public string? numero { get; set; }
    public DateTimeOffset fecha_creacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset fecha_emision { get; set; }
    public DateTimeOffset? fecha_certificacion { get; set; }
    public string xml_enviado { get; set; } = string.Empty;
    public string? xml_certificado { get; set; }
    public string? respuesta { get; set; }
    public byte[] version { get; set; } = [];
    public Ventas Venta { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
