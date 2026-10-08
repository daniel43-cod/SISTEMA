using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Models;

[Table("sesiones_usuario")]
public class SesionUsuario
{
    [Key]
    [Column("id_sesion")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid IdSesion { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    // Almacena SHA-256 del JWT emitido, nunca el token original.
    [Required]
    [Column("token_hash", TypeName = "binary(32)")]
    [JsonIgnore]
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();

    // El servicio de autenticación asignará estas fechas en UTC.
    [Column("fecha_creacion", TypeName = "datetime2")]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_vencimiento", TypeName = "datetime2")]
    public DateTime FechaVencimiento { get; set; }

    [Column("fecha_revocacion", TypeName = "datetime2")]
    public DateTime? FechaRevocacion { get; set; }

    [MaxLength(20)]
    [Column("motivo_cierre", TypeName = "varchar(20)")]
    public string? MotivoCierre { get; set; }

    // SQL Server genera este valor para detectar actualizaciones simultáneas.
    [Timestamp]
    [Column("row_version")]
    [JsonIgnore]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [JsonIgnore]
    public Usuario Usuario { get; set; } = null!;
}
