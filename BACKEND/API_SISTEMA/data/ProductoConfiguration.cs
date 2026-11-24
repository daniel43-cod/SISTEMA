using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_SISTEMA.Data;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Productos>
{
    public void Configure(EntityTypeBuilder<Productos> b)
    {
        b.ToTable("productos", tabla =>
            tabla.HasCheckConstraint("CK_productos_stock_minimo", "stock_minimo >= 0"));
        b.HasKey(p => p.IdProducto);
        b.Property(p => p.IdProducto).ValueGeneratedOnAdd();
        b.Property(p => p.CodigoBarra).HasMaxLength(100);
        b.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
        b.Property(p => p.Imagen).HasMaxLength(2048);
        b.Property(p => p.PrecioPromedioCompra).HasPrecision(18, 6);
        b.Property(p => p.CostoUnitario).HasPrecision(18, 2);
        b.Property(p => p.Impuesto).HasPrecision(18, 2);
        // Estas fechas representan hora civil de Guatemala, no un instante UTC.
        b.Property(p => p.FechaCreacion).HasColumnType("timestamp without time zone");
        b.HasOne(p => p.Marca).WithMany().HasForeignKey(p => p.IdMarca)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PresentacionConfiguration : IEntityTypeConfiguration<Presentacion>
{
    public void Configure(EntityTypeBuilder<Presentacion> b)
    {
        b.ToTable("presentaciones");
        b.HasKey(p => p.IdPresentacion);
        b.Property(p => p.IdPresentacion).ValueGeneratedOnAdd();
        b.Property(p => p.Descripcion).HasMaxLength(100);
    }
}

public sealed class ProductoPresentacionConfiguration : IEntityTypeConfiguration<ProductoPresentacion>
{
    public void Configure(EntityTypeBuilder<ProductoPresentacion> b)
    {
        // El nombre anterior tenía un espacio final: no crear esa tabla accidentalmente en PostgreSQL.
        b.ToTable("producto_presentacion", tabla =>
        {
            tabla.HasCheckConstraint("CK_producto_presentaciones_unidades", "unidades_equivalentes > 0");
            tabla.HasCheckConstraint("CK_producto_presentacion_propietario",
                "(id_producto IS NOT NULL AND id_surtido IS NULL) OR (id_producto IS NULL AND id_surtido IS NOT NULL)");
        });
        b.HasKey(p => p.id_producto_presentacion);
        b.Property(p => p.id_producto_presentacion).ValueGeneratedOnAdd();
        b.Property(p => p.precio).HasPrecision(18, 2);
        b.HasOne(p => p.Producto).WithMany(p => p.ProductoPresentaciones)
            .HasForeignKey(p => p.id_producto).IsRequired(false);
        b.HasOne(p => p.Surtido).WithMany()
            .HasForeignKey(p => p.id_surtido).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.Presentacion).WithMany().HasForeignKey(p => p.IdPresentacion)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
