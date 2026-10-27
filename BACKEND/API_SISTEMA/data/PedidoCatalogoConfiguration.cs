using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_SISTEMA.Data;

public sealed class ZonaEntregaConfiguration : IEntityTypeConfiguration<ZonaEntrega>
{
    public void Configure(EntityTypeBuilder<ZonaEntrega> b)
    {
        b.Property(e => e.IdZonaEntrega).HasColumnName("id_zona_entrega");
        b.Property(e => e.MinimoCompra).HasColumnName("minimo_compra");
        b.Property(e => e.Nombre).HasColumnName("nombre");
        b.Property(e => e.Estado).HasColumnName("estado");
        b.ToTable("zona_entrega", "dbo", t =>
        {
            t.HasCheckConstraint("CK_zona_entrega_nombre", "LEN(LTRIM(RTRIM(nombre))) > 0");
            t.HasCheckConstraint("CK_zona_entrega_minimo", "minimo_compra > 0");
        });
        b.HasKey(z => z.IdZonaEntrega).HasName("PK_zona_entrega");
        b.Property(z => z.IdZonaEntrega).UseIdentityColumn();
        b.Property(z => z.Nombre).HasMaxLength(100).IsRequired();
        b.Property(z => z.MinimoCompra).HasPrecision(18, 2);
        b.Property(z => z.Estado).HasDefaultValue(true).HasSentinel(true);
        b.HasAlternateKey(z => z.Nombre).HasName("UQ_zona_entrega_nombre");
    }
}

public sealed class PedidoCatalogoConfiguration : IEntityTypeConfiguration<PedidoCatalogo>
{
    public void Configure(EntityTypeBuilder<PedidoCatalogo> b)
    {
        b.Property(e => e.IdZonaEntrega).HasColumnName("id_zona_entrega");
        b.Property(e => e.NombreCliente).HasColumnName("nombre_cliente");
        b.Property(e => e.IdOperacion).HasColumnName("id_operacion");
        b.Property(e => e.IdPedido).HasColumnName("id_pedido");
        b.Property(e => e.FechaUtc).HasColumnName("fecha_utc");
        b.Property(e => e.IdVenta).HasColumnName("id_venta");
        b.Property(e => e.Telefono).HasColumnName("telefono");
        b.Property(e => e.Direccion).HasColumnName("direccion");
        b.Property(e => e.Referencias).HasColumnName("referencias");
        b.Property(e => e.Observacion).HasColumnName("observacion");
        b.Property(e => e.Estado).HasColumnName("estado");
        b.Property(e => e.Total).HasColumnName("total");
        b.ToTable("pedido_catalogo", "dbo", t =>
        {
            t.HasCheckConstraint("CK_pedido_catalogo_contacto", "LEN(LTRIM(RTRIM(nombre_cliente))) > 0 AND LEN(LTRIM(RTRIM(telefono))) > 0 AND LEN(LTRIM(RTRIM(direccion))) > 0");
            t.HasCheckConstraint("CK_pedido_catalogo_total", "total > 0");
            t.HasCheckConstraint("CK_pedido_catalogo_estado", "estado IN (0, 1, 2, 3, 4)");
            t.HasCheckConstraint("CK_pedido_catalogo_conversion", "(estado = 4 AND id_venta IS NOT NULL) OR (estado <> 4 AND id_venta IS NULL)");
        });
        b.HasKey(p => p.IdPedido).HasName("PK_pedido_catalogo");
        b.Property(p => p.IdPedido).UseIdentityColumn();
        b.HasAlternateKey(p => p.IdOperacion).HasName("UQ_pedido_catalogo_operacion");
        b.Property(p => p.NombreCliente).HasMaxLength(150).IsRequired();
        b.Property(p => p.Telefono).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.Direccion).HasMaxLength(300).IsRequired();
        b.Property(p => p.Referencias).HasMaxLength(300);
        b.Property(p => p.Observacion).HasMaxLength(500);
        b.Property(p => p.FechaUtc).HasPrecision(3).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(p => p.Total).HasPrecision(18, 2);
        b.Property(p => p.Estado).HasConversion<byte>().HasDefaultValue(EstadoPedidoCatalogo.Pendiente);
        b.HasOne(p => p.ZonaEntrega).WithMany(z => z.Pedidos)
            .HasForeignKey(p => p.IdZonaEntrega).OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_pedido_catalogo_zona");
        b.HasOne(p => p.Venta).WithOne().HasForeignKey<PedidoCatalogo>(p => p.IdVenta)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_pedido_catalogo_venta");
        b.HasIndex(p => p.IdVenta).IsUnique().HasFilter("[id_venta] IS NOT NULL")
            .HasDatabaseName("UX_pedido_catalogo_venta");
        b.HasIndex(p => new { p.Estado, p.FechaUtc, p.IdPedido }).IsDescending(false, true, true)
            .HasDatabaseName("IX_pedido_catalogo_estado_fecha");
    }
}

public sealed class DetallePedidoCatalogoConfiguration : IEntityTypeConfiguration<DetallePedidoCatalogo>
{
    public void Configure(EntityTypeBuilder<DetallePedidoCatalogo> b)
    {
        b.Property(e => e.IdProductoPresentacion).HasColumnName("id_producto_presentacion");
        b.Property(e => e.IdDetallePedido).HasColumnName("id_detalle_pedido");
        b.Property(e => e.NombrePresentacion).HasColumnName("nombre_presentacion");
        b.Property(e => e.NombreProducto).HasColumnName("nombre_producto");
        b.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario");
        b.Property(e => e.IdPedido).HasColumnName("id_pedido");
        b.Property(e => e.Cantidad).HasColumnName("cantidad");
        b.Property(e => e.Subtotal).HasColumnName("subtotal");
        b.ToTable("detalle_pedido_catalogo", "dbo", t =>
        {
            t.HasCheckConstraint("CK_detalle_pedido_catalogo_importes", "cantidad > 0 AND precio_unitario > 0");
            t.HasCheckConstraint("CK_detalle_pedido_catalogo_nombres", "LEN(LTRIM(RTRIM(nombre_producto))) > 0 AND LEN(LTRIM(RTRIM(nombre_presentacion))) > 0");
        });
        b.HasKey(d => d.IdDetallePedido).HasName("PK_detalle_pedido_catalogo");
        b.Property(d => d.IdDetallePedido).UseIdentityColumn();
        b.Property(d => d.NombreProducto).HasMaxLength(200).IsRequired();
        b.Property(d => d.NombrePresentacion).HasMaxLength(150).IsRequired();
        b.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
        b.Property(d => d.Subtotal).HasPrecision(18, 2)
            .HasComputedColumnSql("CONVERT(DECIMAL(18,2), cantidad * precio_unitario)", stored: true);
        b.HasOne(d => d.Pedido).WithMany(p => p.Detalles).HasForeignKey(d => d.IdPedido)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_detalle_pedido_catalogo_pedido");
        b.HasOne(d => d.ProductoPresentacion).WithMany().HasForeignKey(d => d.IdProductoPresentacion)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_detalle_pedido_catalogo_presentacion");
        b.HasAlternateKey(d => new { d.IdPedido, d.IdProductoPresentacion })
            .HasName("UQ_detalle_pedido_catalogo_presentacion");
    }
}
