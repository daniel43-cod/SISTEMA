using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using System.Net.Security;
using System.Runtime.InteropServices.Marshalling;

namespace API_SISTEMA.Data
{
    public class SistemaDbContext :DbContext
    {


        //constructor de la clase
        public SistemaDbContext(
            DbContextOptions<SistemaDbContext> options)
            : base(options)
        {
        }

        //funcion del dbset?
        public DbSet<Categoria> categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<CuentaCliente> CuentaClientes {get;set;}
        public DbSet<Conversacion> Conversaciones {get;set;}
        public DbSet<Mensaje> Mensajes {get;set;}
        public DbSet<Cliente> cliente { get; set; }
        public DbSet<DetalleVenta> detalle_Ventas { get; set; }
        public DbSet<InventarioMovimiento> inventario_Movimientos { get; set; }
        public DbSet<Pagos> pagos { get; set; }
        public DbSet<ProductoPrecio> producto_precios { get; set; }
        public DbSet<API_SISTEMA.Models.Productos> productos { get; set; }
        public DbSet<Proveedores> proveedores { get; set;}
        public DbSet<Rol> rols { get; set; }
        public DbSet<RolPermiso> rol_Permisocs { get; set; }
        public DbSet<TablaPermiso> tabla_Permisos { get; set; }
        public DbSet<Usuario> usuarios { get; set; }
        public DbSet<SesionUsuario> SesionesUsuario { get; set; }
        public DbSet<AuditoriaEvento> AuditoriaEventos { get; set; }
        public DbSet<AuditoriaEventoDetalle> AuditoriaEventoDetalles { get; set; }
        public DbSet<VentaOperacion> VentaOperaciones { get; set; }
        public DbSet<DocumentoFel> DocumentosFel { get; set; }
        public DbSet<DocumentoFelIntento> DocumentosFelIntentos { get; set; }
        public DbSet<WhatsAppEnvio> WhatsAppEnvios { get; set; }
        public DbSet<WhatsAppEnvioPagina> WhatsAppEnvioPaginas { get; set; }
        public DbSet<WhatsAppEnvioIntento> WhatsAppEnvioIntentos { get; set; }
        public DbSet<WhatsAppWebhookEvento> WhatsAppWebhookEventos { get; set; }
        public DbSet<Ventas> ventas { get; set; }
        public DbSet<ZonaEntrega> ZonasEntrega { get; set; }
        public DbSet<PedidoCatalogo> PedidosCatalogo { get; set; }
        public DbSet<DetallePedidoCatalogo> DetallesPedidoCatalogo { get; set; }
        public DbSet<TipoCliente> tipo_cliente { get; set; }
        public DbSet<EstadoVenta> estado_venta { get; set; }
        public DbSet<ProductoPresentacion> producto_presentaciones { get; set; }
        public DbSet<Presentacion> presentaciones { get; set; }
        public DbSet<DetalleCompra> detalle_compras { get; set; }
        public DbSet<RegistroCompras> registroCompras { get; set; }
        public DbSet<EstadoCompra> estado_compras { get; set; }
        public DbSet<PagosCompra> pagosCompras { get; set; }
        public DbSet<Empresa> empresa { get; set; }
        public DbSet<API_SISTEMA.Models.Caja> caja { get; set; }
        public DbSet<SesionCaja> sesioncaja { get; set; }
        public DbSet<TipoMovimientoCaja> tipomovimientocaja {  get; set; }
        public DbSet<MovimientoCaja> movimientocaja { get; set; }
        public DbSet<Gastos> gastos { get; set; }
        public DbSet<UsuarioPermiso> usuario_permisos { get; set; }




        // Mapeo relacional para PostgreSQL.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AuditoriaEventoConfiguration());
            WhatsAppEnvioConfiguration.Configurar(modelBuilder);
            modelBuilder.ApplyConfiguration(new DocumentoFelConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentoFelIntentoConfiguration());
            modelBuilder.ApplyConfiguration(new ProductoConfiguration());
            modelBuilder.ApplyConfiguration(new PresentacionConfiguration());
            modelBuilder.ApplyConfiguration(new ProductoPresentacionConfiguration());
            modelBuilder.ApplyConfiguration(new ZonaEntregaConfiguration());
            modelBuilder.ApplyConfiguration(new PedidoCatalogoConfiguration());
            modelBuilder.ApplyConfiguration(new DetallePedidoCatalogoConfiguration());

            modelBuilder.Entity<AuditoriaEventoDetalle>(detalle =>
            {
                detalle.ToTable("auditoria_evento_detalle", "public", tabla =>
                    tabla.HasCheckConstraint("CK_auditoria_detalle_campo", "length(btrim(campo)) > 0"));
                detalle.HasOne(d => d.AuditoriaEvento)
                    .WithMany(e => e.Detalles)
                    .HasForeignKey(d => d.IdAuditoria)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_auditoria_detalle_evento");
                detalle.HasAlternateKey(d => new { d.IdAuditoria, d.Campo })
                    .HasName("UQ_auditoria_detalle_campo");
            });

            modelBuilder.Entity<Categoria>().ToTable("categoria");
            modelBuilder.Entity<Marca>().ToTable("marca");

            modelBuilder.Entity<Cliente>().ToTable("cliente");

            modelBuilder.Entity<Ventas>().ToTable("ventas");
            modelBuilder.Entity<VentaOperacion>().ToTable("venta_operacion");
            modelBuilder.Entity<VentaOperacion>().HasKey(o => o.id_operacion);
            modelBuilder.Entity<VentaOperacion>().Property(o => o.huella).HasMaxLength(64).IsUnicode(false);
            modelBuilder.Entity<VentaOperacion>().HasOne(o => o.Venta).WithMany().HasForeignKey(o => o.id_venta).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<VentaOperacion>().HasIndex(o => o.id_venta).IsUnique();
            modelBuilder.Entity<DetalleVenta>().Property(d => d.costo_unitario_venta).HasPrecision(18, 6);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.ganancia).HasPrecision(18, 6);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.impuesto).HasPrecision(18, 6);
            modelBuilder.Entity<Ventas>().Property(v => v.impuesto).HasPrecision(18, 6);
            modelBuilder.Entity<Ventas>().Property(v => v.ganancia_total).HasPrecision(18, 6);
            modelBuilder.Entity<Ventas>().Property(v => v.subtotal).HasPrecision(18, 2);
            modelBuilder.Entity<Ventas>().Property(v => v.descuento).HasPrecision(18, 2);
            modelBuilder.Entity<Ventas>().Property(v => v.total).HasPrecision(18, 2);
            modelBuilder.Entity<Ventas>().Property(v => v.monto_pagado).HasPrecision(18, 2);
            modelBuilder.Entity<Ventas>().Property(v => v.saldo_pendiente).HasPrecision(18, 2);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.precio).HasPrecision(18, 2);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.descuento).HasPrecision(18, 2);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.subtotal).HasPrecision(18, 2);
            modelBuilder.Entity<Pagos>().Property(p => p.monto).HasPrecision(18, 2);
            modelBuilder.Entity<Ventas>().HasIndex(v => new { v.id_sesion_caja, v.fecha_venta });

            modelBuilder.Entity<DetalleVenta>().ToTable("detalle_venta");

            modelBuilder.Entity<Usuario>().ToTable("usuario");

            modelBuilder.Entity<Rol>().ToTable("rol");

            modelBuilder.Entity<RolPermiso>().ToTable("rol_permiso");

            modelBuilder.Entity<ProductoPrecio>().ToTable("producto_precio");

            modelBuilder.Entity<Pagos>().ToTable("pagos");
            modelBuilder.Entity<Pagos>().HasIndex(p => p.id_operacion).IsUnique().HasDatabaseName("UX_pagos_id_operacion").HasFilter("id_operacion IS NOT NULL");

            modelBuilder.Entity<TipoCliente>().ToTable("tipo_cliente");

            modelBuilder.Entity<EstadoVenta>().ToTable("estado_venta");

            modelBuilder.Entity<DetalleCompra>().ToTable("detalle_compra");

            modelBuilder.Entity<RegistroCompras>().ToTable("registro_compras");

            modelBuilder.Entity<RegistroCompras>().Property(c => c.TotalCompra).HasPrecision(10, 2);
            modelBuilder.Entity<RegistroCompras>().Property(c => c.SaldoPendiente).HasPrecision(10, 2);
            modelBuilder.Entity<DetalleCompra>().Property(c => c.precio).HasPrecision(10, 2);
            modelBuilder.Entity<DetalleCompra>().Property(c => c.subtotal).HasPrecision(10, 2);
            modelBuilder.Entity<PagosCompra>().Property(c => c.monto).HasPrecision(10, 2);
            modelBuilder.Entity<Proveedores>().ToTable("proveedores");

            modelBuilder.Entity<EstadoCompra>().ToTable("estado_compra");

            modelBuilder.Entity<Empresa>().ToTable("empresa");

            modelBuilder.Entity<PagosCompra>().ToTable("pagos_compra");
            modelBuilder.Entity<PagosCompra>().Property(p => p.IdOperacion).HasColumnName("id_operacion");
            modelBuilder.Entity<PagosCompra>().HasIndex(p => p.IdOperacion).IsUnique()
                .HasDatabaseName("UX_pagos_compra_id_operacion").HasFilter("id_operacion IS NOT NULL");

            modelBuilder.Entity<API_SISTEMA.Models.Caja>().ToTable("caja");

            modelBuilder.Entity<MovimientoCaja>().ToTable("movimiento_caja");

            modelBuilder.Entity<SesionCaja>().ToTable("sesion_caja");
            // NULLS NOT DISTINCT garantiza una sola sesión abierta global.
            modelBuilder.Entity<SesionCaja>().HasIndex(s => s.fecha_cierre).IsUnique().AreNullsDistinct(false)
                .HasFilter("fecha_cierre IS NULL").HasDatabaseName("UX_sesion_caja_unica_abierta");
            modelBuilder.Entity<SesionCaja>().Property(s => s.monto_inicial).HasPrecision(10, 2);
            modelBuilder.Entity<SesionCaja>().Property(s => s.monto_esperado).HasPrecision(10, 2);
            modelBuilder.Entity<SesionCaja>().Property(s => s.monto_contado).HasPrecision(10, 2);
            modelBuilder.Entity<SesionCaja>().Property(s => s.diferencia).HasPrecision(10, 2);
            modelBuilder.Entity<MovimientoCaja>().Property(s => s.monto).HasPrecision(10, 2);

            modelBuilder.Entity<TipoMovimientoCaja>().ToTable("tipo_movimiento_caja");

            modelBuilder.Entity<Gastos>().ToTable("gastos");

            modelBuilder.Entity<UsuarioPermiso>().ToTable("usuario_permiso");

            modelBuilder.Entity<RolPermiso>().ToTable("rol_permiso");

            modelBuilder.Entity<TablaPermiso>().ToTable("tabla_permisos");

            modelBuilder.Entity<UsuarioPermiso>().ToTable("usuario_permiso");

            modelBuilder.Entity<RolPermiso>().ToTable("rol_permiso");

            modelBuilder.Entity<CuentaCliente>().ToTable("cuenta_cliente");

            modelBuilder.Entity<Conversacion>().ToTable("conversacion");

            modelBuilder.Entity<Mensaje>().ToTable("mensaje");



            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.id_rol);

            // Configuración de la relación Rol_permisocs <-> Tabla_permiso
            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.id_permiso);

            modelBuilder.Entity<ProductoPrecio>()
                .HasOne(p => p.Producto)
                .WithMany(x => x.ProductoPrecios)
                .HasForeignKey(p => p.id_producto);

            modelBuilder.Entity<ProductoPrecio>()
                .HasOne(p => p.TipoCliente)
                .WithMany(x => x.ProductoPrecios)
                .HasForeignKey(p => p.id_tipo_cliente);



            modelBuilder.Entity<Ventas>()
                .HasOne(v => v.cliente)
                .WithMany()
                .HasForeignKey(v => v.id_cliente);

            modelBuilder.Entity<Ventas>()
                .HasOne(v => v.usuario)
                .WithMany()
                .HasForeignKey(v => v.id_usuario);

            modelBuilder.Entity<Ventas>()
                .HasOne(v => v.EstadoVenta)
                .WithMany((e => e.Ventas))
                .HasForeignKey(v => v.id_estado_venta);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(v => v.producto_presentacion)
                .WithMany()
                .HasForeignKey(v => v.id_producto_presentacion);

            modelBuilder.Entity<RegistroCompras>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.IdUsuario);

            modelBuilder.Entity<RegistroCompras>()
                .HasOne(v => v.Proveedores)
                .WithMany()
                .HasForeignKey(v => v.IdProveedor);

            modelBuilder.Entity<RegistroCompras>()
                .HasOne(v => v.EstadoCompra)
                .WithMany()
                .HasForeignKey(v => v.IdEstadoCompra);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(v => v.Productos)
                .WithMany()
                .HasForeignKey(v => v.id_producto);

            modelBuilder.Entity<SesionCaja>()
                .HasOne(v => v.caja)
                .WithMany()
                .HasForeignKey(v => v.id_caja);

            modelBuilder.Entity<SesionCaja>()
                .HasOne(s => s.usuarioapertura)
                .WithMany()
                .HasForeignKey(s => s.id_usuario_apertura)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SesionCaja>()
                .HasOne(s => s.usuariocierre)
                .WithMany()
                .HasForeignKey(s => s.id_usuario_cierre)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.sesionCaja)
                .WithMany()
                .HasForeignKey(v => v.id_sesion_caja);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.tipoMovimientoCaja)
                .WithMany()
                .HasForeignKey(v => v.id_tipo_movimiento);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.usuario)
                .WithMany()
                .HasForeignKey(v => v.id_usuario);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.venta)
                .WithMany()
                .HasForeignKey(v => v.id_venta);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.RegistroCompras)
                .WithMany()
                .HasForeignKey(v => v.id_compra);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.pagos)
                .WithMany()
                .HasForeignKey(v => v.id_pago_venta);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(v => v.pagosCompra)
                .WithMany()
                .HasForeignKey(v => v.id_pago_compra);

            modelBuilder.Entity<Ventas>()
                .HasOne(v => v.sesionCaja)
                .WithMany()
                .HasForeignKey(v => v.id_sesion_caja);

            modelBuilder.Entity<Gastos>()
                .HasOne(v => v.usuario)
                .WithMany()
                .HasForeignKey(v => v.id_usuario);

            modelBuilder.Entity<Gastos>()
                .HasOne(v => v.sesionCaja)
                .WithMany()
                .HasForeignKey(v => v.id_sesion_caja);

            modelBuilder.Entity<Usuario>()
             .HasOne(v => v.rol)
             .WithMany()
             .HasForeignKey(v => v.id_rol);

            modelBuilder.Entity<PagosCompra>()
                .HasOne(v => v.sesioncaja)
                .WithMany()
                .HasForeignKey(v => v.id_sesion_caja);

            modelBuilder.Entity<Pagos>()
                .HasOne(v => v.sesionCaja)
                .WithMany()
                .HasForeignKey(v => v.id_sesion_caja);

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.id_usuario);

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(v => v.tabla_permiso)
                .WithMany()
                .HasForeignKey(v => v.id_permiso);

            modelBuilder.Entity<Conversacion>()
                .HasOne(v => v.CuentaCliente)
                .WithMany()
                .HasForeignKey(v => v.IdCuentaCliente);

            modelBuilder.Entity<Conversacion>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensaje>()
                .HasOne(v => v.Conversacion)
                .WithMany()
                .HasForeignKey(v => v.IdConversacion);



            // Una categoría puede tener muchas marcas; cada marca tiene una sola categoría.
            // Impide borrar una categoría con marcas asociadas y evita borrarlas en cascada.
            modelBuilder.Entity<Marca>()
                .HasOne(m => m.Categoria)
                .WithMany()
                .HasForeignKey(m => m.IdCategoria)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Una marca tiene muchos productos; cada producto requiere una marca.
            // Impide borrar una marca con productos asociados.


            // Un usuario puede tener varias sesiones; cada sesión pertenece a un usuario.
            modelBuilder.Entity<SesionUsuario>()
                .HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.IdUsuario)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Evita hashes repetidos y facilita localizar y limpiar sesiones vencidas.
            modelBuilder.Entity<SesionUsuario>()
                .HasIndex(s => s.TokenHash).IsUnique();
            modelBuilder.Entity<SesionUsuario>()
                .HasIndex(s => s.FechaVencimiento);
            modelBuilder.Entity<SesionUsuario>().ToTable("sesiones_usuario", table =>
                table.HasCheckConstraint("CK_sesiones_usuario_motivo_cierre",
                    "motivo_cierre IN ('VOLUNTARIO', 'EXPIRACION')"));
            PostgreSqlConfiguration.Configurar(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }
    }
}
