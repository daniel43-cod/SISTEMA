using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_SISTEMA.Data;

public sealed class DocumentoFelConfiguration : IEntityTypeConfiguration<DocumentoFel>
{
    public void Configure(EntityTypeBuilder<DocumentoFel> b)
    {
        b.ToTable("documento_fel", "dbo", t =>
        {
            t.HasCheckConstraint("CK_documento_fel_ambiente", "ambiente IN ('PRUEBAS', 'PRODUCCION')");
            t.HasCheckConstraint("CK_documento_fel_tipo", "tipo_dte = 'FACT'");
            t.HasCheckConstraint("CK_documento_fel_estado", "estado IN ('PENDIENTE', 'EN_PROCESO', 'CERTIFICADA', 'RECHAZADA', 'POR_CONFIRMAR')");
            t.HasCheckConstraint("CK_documento_fel_certificada", "estado <> 'CERTIFICADA' OR (uuid IS NOT NULL AND serie IS NOT NULL AND numero IS NOT NULL AND xml_certificado IS NOT NULL AND fecha_certificacion IS NOT NULL)");
        });
        b.HasKey(d => d.id_documento_fel);
        b.Property(d => d.ambiente).HasMaxLength(10).IsUnicode(false);
        b.Property(d => d.tipo_dte).HasMaxLength(4).IsUnicode(false);
        b.Property(d => d.estado).HasMaxLength(15).IsUnicode(false);
        b.Property(d => d.uuid).HasMaxLength(36).IsUnicode(false);
        b.Property(d => d.serie).HasMaxLength(8).IsUnicode(false);
        b.Property(d => d.numero).HasMaxLength(30).IsUnicode(false);
        b.Property(d => d.version).IsRowVersion();
        b.HasIndex(d => new { d.id_venta, d.ambiente }).IsUnique().HasDatabaseName("UX_documento_fel_venta_ambiente");
        b.HasIndex(d => d.identificador).IsUnique().HasDatabaseName("UX_documento_fel_identificador");
        b.HasIndex(d => d.uuid).IsUnique().HasFilter("[uuid] IS NOT NULL").HasDatabaseName("UX_documento_fel_uuid");
        b.HasIndex(d => d.id_usuario).HasDatabaseName("IX_documento_fel_usuario");
        b.HasOne(d => d.Venta).WithMany().HasForeignKey(d => d.id_venta).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.id_usuario).OnDelete(DeleteBehavior.NoAction);
    }
}
