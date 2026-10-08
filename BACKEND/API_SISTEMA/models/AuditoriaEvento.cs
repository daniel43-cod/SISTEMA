using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_SISTEMA.models;

[Table("auditoria_evento", Schema = "dbo")]
public sealed class AuditoriaEvento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_auditoria")]
    public long IdAuditoria { get; set; }

    [Column("fecha_utc", TypeName = "datetime2(7)")]
    public DateTime FechaUtc { get; set; }

    [Column("id_usuario")]
    public int? IdUsuario { get; set; }

    [MaxLength(100)]
    [Column("usuario_responsable")]
    public string? UsuarioResponsable { get; set; }

    [MaxLength(100)]
    [Column("identificador_intentado")]
    public string? IdentificadorIntentado { get; set; }

    [Required, MaxLength(100)]
    [Column("accion", TypeName = "varchar(100)")]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("entidad", TypeName = "varchar(100)")]
    public string? Entidad { get; set; }

    [MaxLength(100)]
    [Column("id_registro")]
    public string? IdRegistro { get; set; }

    [Required, MaxLength(12)]
    [Column("resultado", TypeName = "varchar(12)")]
    public string Resultado { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column("origen", TypeName = "varchar(20)")]
    public string Origen { get; set; } = "API";

    [MaxLength(500)]
    [Column("motivo")]
    public string? Motivo { get; set; }

    [MaxLength(128)]
    [Column("trace_id", TypeName = "varchar(128)")]
    public string? TraceId { get; set; }

    [MaxLength(45)]
    [Column("direccion_ip", TypeName = "varchar(45)")]
    public string? DireccionIp { get; set; }

    [JsonIgnore]
    public Usuario? Usuario { get; set; }

    public ICollection<AuditoriaEventoDetalle> Detalles { get; set; } = new List<AuditoriaEventoDetalle>();
}
