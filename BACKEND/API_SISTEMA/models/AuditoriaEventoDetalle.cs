using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Models;

[Table("auditoria_evento_detalle", Schema = "dbo")]
public sealed class AuditoriaEventoDetalle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_detalle")]
    public long IdDetalle { get; set; }

    [Column("id_auditoria")]
    public long IdAuditoria { get; set; }

    [Required, MaxLength(100)]
    [Column("campo")]
    public string Campo { get; set; } = string.Empty;

    [Column("valor_anterior", TypeName = "nvarchar(max)")]
    public string? ValorAnterior { get; set; }

    [Column("valor_nuevo", TypeName = "nvarchar(max)")]
    public string? ValorNuevo { get; set; }

    [JsonIgnore]
    public AuditoriaEvento AuditoriaEvento { get; set; } = null!;
}
