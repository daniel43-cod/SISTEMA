using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;

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
        public DbSet<Ventas> ventas { get; set; }
        public DbSet<TipoCliente> tipo_cliente { get; set; }
        public DbSet<EstadoVenta> estado_venta { get; set; }
        public DbSet<Producto_Presentacion> producto_presentaciones { get; set; }
        public DbSet<DetalleCompra> detalle_compras { get; set; }
        public DbSet<RegistroCompras> registroCompras { get; set; }
        public DbSet<EstadoCompra> estado_compras { get; set; }
        public DbSet<PagosCompra> pagosCompras { get; set; }
        public DbSet<Empresa> empresa { get; set; }
        public DbSet<caja> caja { get; set; }
        public DbSet<SesionCaja> sesioncaja { get; set; }
        public DbSet<TipoMovimientoCaja> tipomovimientocaja {  get; set; }
        public DbSet<MovimientoCaja> movimientocaja { get; set; }




        //mapear las tablas en SQLserver
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Categoria>().ToTable("categoria");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>().ToTable("cliente");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ventas>().ToTable("ventas");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Productos>().ToTable("productos");
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

            modelBuilder.Entity<Producto_Presentacion>().ToTable("producto_presentacion ");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DetalleCompra>().ToTable("detalle_compra");
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<RegistroCompras>().ToTable("registro_compras");
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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoMovimientoCaja>().ToTable("tipo_movimiento_caja");
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
                .HasOne(v => v.usuario)
                .WithMany()
                .HasForeignKey(v => v.id_usuario);

            modelBuilder.Entity<RegistroCompras>()
                .HasOne(v => v.empresa)
                .WithMany()
                .HasForeignKey(v => v.id_empresa);

            modelBuilder.Entity<RegistroCompras>()
                .HasOne(v => v.estado_compra)
                .WithMany()
                .HasForeignKey(v => v.id_estado_compra);

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








        }





    }
}
