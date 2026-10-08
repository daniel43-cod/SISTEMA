using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using System.Net.Security;
using System.Runtime.InteropServices.Marshalling;

namespace API_SISTEMA.data
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
        public DbSet<Detalle_venta> detalle_Ventas { get; set; }
        public DbSet<Inventario_movimiento> inventario_Movimientos { get; set; }
        public DbSet<Pagos> pagos { get; set; }
        public DbSet<Producto_precio> producto_precios { get; set; }
        public DbSet<Productos> productos { get; set; }
        public DbSet<Proveedores> proveedores { get; set;}
        public DbSet<Rol> rols { get; set; }    
        public DbSet<Rol_permisocs> rol_Permisocs { get; set; }
        public DbSet<Tabla_permiso> tabla_Permisos { get; set; }
        public DbSet<Usuario> usuarios { get; set; }
        public DbSet<SesionUsuario> SesionesUsuario { get; set; }
        public DbSet<AuditoriaEvento> AuditoriaEventos { get; set; }
        public DbSet<AuditoriaEventoDetalle> AuditoriaEventoDetalles { get; set; }
        public DbSet<Ventas> ventas { get; set; }
        public DbSet<TipoCliente> tipo_cliente { get; set; }
        public DbSet<EstadoVenta> estado_venta { get; set; }
        public DbSet<Producto_Presentacion> producto_presentaciones { get; set; }
        public DbSet<Presentacion> presentaciones { get; set; }
        public DbSet<DetalleCompra> detalle_compras { get; set; }
        public DbSet<RegistroCompras> registroCompras { get; set; }
        public DbSet<EstadoCompra> estado_compras { get; set; }
        public DbSet<PagosCompra> pagosCompras { get; set; }
        public DbSet<Empresa> empresa { get; set; }
        public DbSet<caja> caja { get; set; }
        public DbSet<SesionCaja> sesioncaja { get; set; }
        public DbSet<TipoMovimientoCaja> tipomovimientocaja {  get; set; }
        public DbSet<MovimientoCaja> movimientocaja { get; set; }
        public DbSet<Gastos> gastos { get; set; } 
        public DbSet<usuario_permiso> usuario_permisos { get; set; }




        //mapear las tablas en SQLserver
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AuditoriaEventoConfiguration());

            modelBuilder.Entity<AuditoriaEventoDetalle>(detalle =>
            {
                detalle.ToTable("auditoria_evento_detalle", "dbo", tabla =>
                    tabla.HasCheckConstraint("CK_auditoria_detalle_campo", "LEN(LTRIM(RTRIM(campo))) > 0"));
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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>().ToTable("cliente");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ventas>().ToTable("ventas");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Productos>().ToTable("productos", tabla =>
                tabla.HasCheckConstraint("CK_productos_stock_minimo", "[stock_minimo] >= 0"));
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Presentacion>().ToTable("presentaciones");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Detalle_venta>().ToTable("detalle_venta");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().ToTable("usuario");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>().ToTable("rol");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol_permisocs>().ToTable("rol_permiso");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto_precio>().ToTable("producto_precio");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pagos>().ToTable("pagos");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoCliente>().ToTable("tipo_cliente");
            base.OnModelCreating(modelBuilder); 
            
            modelBuilder.Entity<EstadoVenta>().ToTable("estado_venta");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto_Presentacion>().ToTable("producto_presentacion ", tabla =>
                tabla.HasCheckConstraint("CK_producto_presentaciones_unidades", "[unidades_equivalentes] > 0"));
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DetalleCompra>().ToTable("detalle_compra");
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<RegistroCompras>().ToTable("registro_compras");
            modelBuilder.Entity<RegistroCompras>().Property(c => c.TotalCompra).HasPrecision(10, 2);
            modelBuilder.Entity<RegistroCompras>().Property(c => c.SaldoPendiente).HasPrecision(10, 2);
            modelBuilder.Entity<DetalleCompra>().Property(c => c.precio).HasPrecision(10, 2);
            modelBuilder.Entity<DetalleCompra>().Property(c => c.subtotal).HasPrecision(10, 2);
            modelBuilder.Entity<PagosCompra>().Property(c => c.monto).HasPrecision(10, 2);
            modelBuilder.Entity<Proveedores>().ToTable("proveedores");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EstadoCompra>().ToTable("estado_compra");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Empresa>().ToTable("empresa");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PagosCompra>().ToTable("pagos_compra");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<caja>().ToTable("caja");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MovimientoCaja>().ToTable("movimiento_caja");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SesionCaja>().ToTable("sesion_caja");
            // SQL Server permite un solo NULL en este indice: una unica sesion abierta global.
            modelBuilder.Entity<SesionCaja>().HasIndex(s => s.fecha_cierre).IsUnique()
                .HasFilter("[fecha_cierre] IS NULL").HasDatabaseName("UX_sesion_caja_unica_abierta");
            modelBuilder.Entity<SesionCaja>().Property(s => s.monto_inicial).HasPrecision(10, 2);
            modelBuilder.Entity<SesionCaja>().Property(s => s.monto_esperado).HasPrecision(10, 2);
            modelBuilder.Entity<SesionCaja>().Property(s => s.monto_contado).HasPrecision(10, 2);
            modelBuilder.Entity<SesionCaja>().Property(s => s.diferencia).HasPrecision(10, 2);
            modelBuilder.Entity<MovimientoCaja>().Property(s => s.monto).HasPrecision(10, 2);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoMovimientoCaja>().ToTable("tipo_movimiento_caja");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Gastos>().ToTable("gastos"); 
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<usuario_permiso>().ToTable("usuario_permiso");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol_permisocs>().ToTable("rol_permiso");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tabla_permiso>().ToTable("tabla_permisos"); 
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<usuario_permiso>().ToTable("usuario_permiso");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol_permisocs>().ToTable("rol_permiso");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CuentaCliente>().ToTable("cuenta_cliente");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Conversacion>().ToTable("conversacion");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Mensaje>().ToTable("mensaje");
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<Rol_permisocs>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.id_rol);

            // Configuración de la relación Rol_permisocs <-> Tabla_permiso
            modelBuilder.Entity<Rol_permisocs>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.id_permiso);

            modelBuilder.Entity<Producto_precio>()
                .HasOne(p => p.Producto)
                .WithMany(x => x.ProductoPrecios)
                .HasForeignKey(p => p.id_producto);

            modelBuilder.Entity<Producto_precio>()
                .HasOne(p => p.TipoCliente)
                .WithMany(x => x.ProductoPrecios)
                .HasForeignKey(p => p.id_tipo_cliente);
                base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto_Presentacion>()
               .HasOne(pp => pp.Producto)
               .WithMany(p => p.ProductoPresentaciones)
               .HasForeignKey(pp => pp.id_producto);

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

            modelBuilder.Entity<Detalle_venta>()
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

            modelBuilder.Entity<usuario_permiso>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.id_usuario);
            
            modelBuilder.Entity<usuario_permiso>()
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

            modelBuilder.Entity<Producto_Presentacion>()
                .HasOne(p => p.Presentacion)
                .WithMany()
                .HasForeignKey(p => p.IdPresentacion)
                .OnDelete(DeleteBehavior.Restrict);

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
            modelBuilder.Entity<Productos>()
                .HasOne(p => p.Marca)
                .WithMany()
                .HasForeignKey(p => p.IdMarca)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
        }
    }
}
