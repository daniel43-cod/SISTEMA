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
        b.HasOne(d => d.DetalleVenta).WithMany(d => d.ComponentesSurtido)
            .HasForeignKey(d => d.IdDetalleVenta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.IdProducto).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DetalleVentaPropietarioConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> b)
    {
        b.ToTable("detalle_venta", t => t.HasCheckConstraint("CK_detalle_venta_propietario",
            "(id_producto IS NOT NULL AND id_surtido IS NULL) OR (id_producto IS NULL AND id_surtido IS NOT NULL)"));
        b.HasOne(d => d.Surtido).WithMany().HasForeignKey(d => d.id_surtido).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DetallePedidoSurtidoConfiguration : IEntityTypeConfiguration<DetallePedidoSurtido>
{
    public void Configure(EntityTypeBuilder<DetallePedidoSurtido> b)
    {
        b.ToTable("detalle_pedido_surtido", "public", t => t.HasCheckConstraint("CK_detalle_pedido_surtido_cantidad", "cantidad > 0"));
        b.HasKey(p => new { p.IdDetallePedido, p.IdProducto });
        b.Property(p => p.IdDetallePedido).HasColumnName("id_detalle_pedido");
        b.Property(p => p.IdProducto).HasColumnName("id_producto");
        b.Property(p => p.Cantidad).HasColumnName("cantidad");
        b.Property(p => p.NombreProducto).HasColumnName("nombre_producto").HasMaxLength(200).IsRequired();
        b.HasOne(p => p.Detalle).WithMany(d => d.Componentes).HasForeignKey(p => p.IdDetallePedido).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.Producto).WithMany().HasForeignKey(p => p.IdProducto).OnDelete(DeleteBehavior.Restrict);
    }
}
