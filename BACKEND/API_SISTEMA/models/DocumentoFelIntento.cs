using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.Models;

[Table("documento_fel_intento", Schema = "public")]
public sealed class DocumentoFelIntento
{
    [Key] public long id_intento { get; set; }
    public int id_documento_fel { get; set; }
    public int id_usuario { get; set; }
    public Guid identificador { get; set; }
    [Column(TypeName = "varchar(15)")] public string estado { get; set; } = "EN_PROCESO";
    public DateTimeOffset fecha_creacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset fecha_emision { get; set; }
    public DateTimeOffset? fecha_respuesta { get; set; }
    // Control de procesamiento local, separado del identificador enviado a INFILE.
    // NULL en intentos anteriores: todavía no tenían este mecanismo.
    public DateTimeOffset? procesamiento_inicio { get; set; }
    public DateTimeOffset? procesamiento_vence { get; set; }
    public Guid? procesamiento_token { get; set; }
    public int recuperaciones { get; set; }
    public DateTimeOffset? ultima_recuperacion { get; set; }
    public short? codigo_http { get; set; }
    public bool? resultado { get; set; }
    public string? origen { get; set; }
    public string? descripcion { get; set; }
    public string? uuid_respuesta { get; set; }
    public string? serie_respuesta { get; set; }
    public string? numero_respuesta { get; set; }
    public long? saldo { get; set; }
    public long? creditos { get; set; }
    public long? cupo_diario { get; set; }
    public long? saldo_diario { get; set; }
    public bool? alertas_infile { get; set; }
    public bool? alertas_sat { get; set; }
    public int? cantidad_errores { get; set; }
    public string xml_enviado { get; set; } = "";
    public string? xml_certificado { get; set; }
    public bool migrado { get; set; }
    [Timestamp] public uint version { get; set; }
    public List<DocumentoFelError> Errores { get; set; } = [];
    public List<DocumentoFelAlerta> Alertas { get; set; } = [];
}

[Table("documento_fel_error", Schema = "public")]
public sealed class DocumentoFelError
{
    [Key] public long id_error { get; set; }
    public long id_intento { get; set; }
    public int orden { get; set; }
    public bool? resultado { get; set; }
    public string? fuente { get; set; }
    public string? categoria { get; set; }
    public string? numeral { get; set; }
    public string? validacion { get; set; }
    public string? mensaje { get; set; }
}

[Table("documento_fel_alerta", Schema = "public")]
public sealed class DocumentoFelAlerta
{
    [Key] public long id_alerta { get; set; }
    public long id_intento { get; set; }
    [Column(TypeName = "varchar(6)")] public string origen { get; set; } = "INFILE";
    public int orden { get; set; }
    public string? fuente { get; set; }
    public string? categoria { get; set; }
    public string? numeral { get; set; }
    public string? validacion { get; set; }
    public string? mensaje { get; set; }
}
