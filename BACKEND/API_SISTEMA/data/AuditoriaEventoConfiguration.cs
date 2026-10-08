using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_SISTEMA.data;

public sealed class AuditoriaEventoConfiguration : IEntityTypeConfiguration<AuditoriaEvento>
{
    public void Configure(EntityTypeBuilder<AuditoriaEvento> builder)
    {
        builder.ToTable("auditoria_evento", "dbo", table =>
        {
            table.HasCheckConstraint("CK_auditoria_evento_accion", "LEN(LTRIM(RTRIM(accion))) > 0");
            table.HasCheckConstraint("CK_auditoria_evento_resultado", "resultado IN ('EXITOSO', 'RECHAZADO', 'FALLIDO')");
            table.HasCheckConstraint("CK_auditoria_evento_origen", "origen IN ('API', 'SISTEMA', 'SQL')");
            table.HasCheckConstraint("CK_auditoria_evento_registro_entidad",
                "id_registro IS NULL OR (entidad IS NOT NULL AND LEN(LTRIM(RTRIM(entidad))) > 0)");
        });
        builder.HasKey(e => e.IdAuditoria).HasName("PK_auditoria_evento");
        builder.Property(e => e.FechaUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.Origen).HasDefaultValue("API");
        builder.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_auditoria_evento_usuario");
        builder.HasIndex(e => new { e.FechaUtc, e.IdAuditoria })
            .IsDescending(true, true).HasDatabaseName("IX_auditoria_evento_fecha");
        builder.HasIndex(e => new { e.IdUsuario, e.FechaUtc })
            .IsDescending(false, true).HasFilter("[id_usuario] IS NOT NULL")
            .HasDatabaseName("IX_auditoria_evento_usuario_fecha");
        builder.HasIndex(e => new { e.Entidad, e.IdRegistro, e.FechaUtc })
            .IsDescending(false, false, true).HasFilter("[id_registro] IS NOT NULL")
            .HasDatabaseName("IX_auditoria_evento_entidad_registro_fecha");
        builder.HasIndex(e => new { e.Accion, e.FechaUtc })
            .IsDescending(false, true).HasDatabaseName("IX_auditoria_evento_accion_fecha");
    }
}
