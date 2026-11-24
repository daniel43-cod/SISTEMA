using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_SISTEMA.Data;

public sealed class SurtidoConfiguration : IEntityTypeConfiguration<Surtido>
{
    public void Configure(EntityTypeBuilder<Surtido> b)
    {
        b.ToTable("surtido", "public", t =>
            t.HasCheckConstraint("CK_surtido_nombre", "length(btrim(nombre)) > 0"));
        b.HasKey(s => s.IdSurtido);
        b.Property(s => s.IdSurtido).ValueGeneratedOnAdd();
        b.Property(s => s.Nombre).HasMaxLength(200).IsRequired();
        b.HasOne(s => s.Marca).WithMany().HasForeignKey(s => s.IdMarca)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SurtidoProductoConfiguration : IEntityTypeConfiguration<SurtidoProducto>
{
    public void Configure(EntityTypeBuilder<SurtidoProducto> b)
    {
        b.HasKey(p => new { p.IdSurtido, p.IdProducto });
        b.HasOne(p => p.Surtido).WithMany(s => s.Productos)
            .HasForeignKey(p => p.IdSurtido).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.Producto).WithMany()
            .HasForeignKey(p => p.IdProducto).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DetalleVentaSurtidoConfiguration : IEntityTypeConfiguration<DetalleVentaSurtido>
{
    public void Configure(EntityTypeBuilder<DetalleVentaSurtido> b)
    {
        b.ToTable("detalle_venta_surtido", "public", t =>
        {
            t.HasCheckConstraint("CK_detalle_venta_surtido_cantidad", "cantidad > 0");
            t.HasCheckConstraint("CK_detalle_venta_surtido_costo", "costo_unitario_venta > 0");
        });
        b.HasKey(d => new { d.IdDetalleVenta, d.IdProducto });
        b.Property(d => d.CostoUnitarioVenta).HasPrecision(18, 6);
        b.HasOne(d => d.DetalleVenta).WithMany()
            .HasForeignKey(d => d.IdDetalleVenta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.IdProducto).OnDelete(DeleteBehavior.Restrict);
    }
}
