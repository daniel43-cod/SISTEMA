using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_SISTEMA.Data;

public sealed class DocumentoFelIntentoConfiguration : IEntityTypeConfiguration<DocumentoFelIntento>
{
    public void Configure(EntityTypeBuilder<DocumentoFelIntento> b)
    {
        b.HasIndex(i => i.identificador).IsUnique();
        b.Property(i => i.recuperaciones).HasDefaultValue(0);
        b.HasIndex(i => i.procesamiento_vence).HasFilter("[estado] = 'EN_PROCESO'")
            .HasDatabaseName("IX_fel_intento_procesamiento_vence");
        b.HasOne<DocumentoFel>().WithMany().HasForeignKey(i => i.id_documento_fel).OnDelete(DeleteBehavior.NoAction);
        b.HasOne<Usuario>().WithMany().HasForeignKey(i => i.id_usuario).OnDelete(DeleteBehavior.NoAction);
        b.HasMany(i => i.Errores).WithOne().HasForeignKey(e => e.id_intento).OnDelete(DeleteBehavior.NoAction);
        b.HasMany(i => i.Alertas).WithOne().HasForeignKey(e => e.id_intento).OnDelete(DeleteBehavior.NoAction);
    }
}
