using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API_SISTEMA.Data;

public static class PostgreSqlConfiguration
{
    public static void Configurar(ModelBuilder model)
    {
        model.HasDefaultSchema("public");
        model.Entity<RegistroCompras>().HasIndex(c => c.IdOperacion).IsUnique().HasDatabaseName("UX_registro_compra_id_operacion");
        model.Entity<Gastos>().HasIndex(g => g.id_operacion).IsUnique().HasDatabaseName("UX_gastos_id_operacion");
        // Identidades de acceso insensibles a mayúsculas; la contraseña conserva comparación exacta.
        model.HasPostgresExtension("citext");
        model.Entity<Usuario>().Property(u => u.usuario).HasColumnType("citext");
        model.Entity<Usuario>().Property(u => u.correo).HasColumnType("citext");
        model.Entity<Usuario>().HasIndex(u => u.usuario).IsUnique().HasDatabaseName("UX_usuario_usuario");
        model.Entity<Usuario>().HasIndex(u => u.correo).IsUnique().HasFilter("correo IS NOT NULL").HasDatabaseName("UX_usuario_correo");
        model.Entity<Usuario>().HasIndex(u => u.telefono).IsUnique().HasDatabaseName("UX_usuario_telefono");
        model.Entity<SesionUsuario>().Property(s => s.RowVersion).HasColumnName("xmin").IsRowVersion();
        model.Entity<DocumentoFel>().Property(d => d.version).HasColumnName("xmin").IsRowVersion();
        model.Entity<DocumentoFelIntento>().Property(d => d.version).HasColumnName("xmin").IsRowVersion();

        // Relaciones financieras explícitas: los IDs escalares por sí solos no crean claves foráneas en EF.
        model.Entity<Pagos>().HasOne<Ventas>().WithMany().HasForeignKey(p => p.id_venta).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Pagos>().HasOne<Usuario>().WithMany().HasForeignKey(p => p.id_usuario).OnDelete(DeleteBehavior.Restrict);
        model.Entity<PagosCompra>().HasOne<RegistroCompras>().WithMany().HasForeignKey(p => p.id_compra).OnDelete(DeleteBehavior.Restrict);
        model.Entity<PagosCompra>().HasOne<Usuario>().WithMany().HasForeignKey(p => p.id_usuario).OnDelete(DeleteBehavior.Restrict);
        model.Entity<DetalleCompra>().HasOne<RegistroCompras>().WithMany().HasForeignKey(d => d.id_registro_compra).OnDelete(DeleteBehavior.Restrict);
        model.Entity<RegistroCompras>().HasOne<SesionCaja>().WithMany().HasForeignKey(c => c.IdSesionCaja).OnDelete(DeleteBehavior.Restrict);
        model.Entity<InventarioMovimiento>().HasOne<Productos>().WithMany().HasForeignKey(m => m.id_producto).OnDelete(DeleteBehavior.Restrict);
        model.Entity<InventarioMovimiento>().HasOne<Usuario>().WithMany().HasForeignKey(m => m.id_usuario).OnDelete(DeleteBehavior.Restrict);
        var instanteUtc = new ValueConverter<DateTimeOffset, DateTimeOffset>(v => v.ToUniversalTime(), v => v);
        foreach (var entity in model.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
        {
            var tipo = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            if (tipo == typeof(DateTime))
            {
                var utc = entity.ClrType == typeof(SesionUsuario) || entity.ClrType == typeof(CuentaCliente) ||
                    property.Name.Contains("Utc", StringComparison.OrdinalIgnoreCase) || property.Name.EndsWith("_utc");
                property.SetColumnType(utc ? "timestamp with time zone" : "timestamp without time zone");
            }
            else if (tipo == typeof(DateTimeOffset))
            {
                // PostgreSQL conserva el instante. El XML fiscal conserva su offset original.
                property.SetColumnType("timestamp with time zone");
                property.SetValueConverter(instanteUtc);
            }
            else if (tipo == typeof(decimal) && property.GetPrecision() is null)
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}
