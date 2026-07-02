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



        //mapear las tablas en SQLserver
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Categoria>().ToTable("categoria");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>().ToTable("cliente");
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


            modelBuilder.Entity<Ventas>()
    .HasOne(v => v.EstadoVenta)
    .WithMany(e => e.Ventas)
    .HasForeignKey(v => v.id_estado_venta);

            modelBuilder.Entity<Ventas>()
    .HasOne(v => v.TipoCliente)
    .WithMany()
    .HasForeignKey(v => v.id_tipo_cliente);
        }
            



}}
