using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class ProductoService
    {
        private readonly SistemaDbContext _context;

        public ProductoService(SistemaDbContext context)
        {
            _context = context;
        }

    

        public async Task<List<ProductoVentaBuscarDTO>> ObtenerTodosProductosVenta()
        {
            var productos = await _context.producto_presentaciones
                .Include(p => p.Producto)
                .Select(p => new ProductoVentaBuscarDTO
                {
                    id_producto = p.id_producto,
                    id_producto_presentacion = p.id_producto_presentacion,
                    nombre_producto = p.Producto.nombre,
                    presentacion = p.descripcion,
                    unidades_equivalentes = p.unidades_equivalentes,
                    precio = p.precio,
                    stock = p.Producto.stock
                })
                .ToListAsync();

            return productos;
        }

        public async Task<List<Producto_Presentacion>> ListarPresentaciones(int idProducto)
        {
            return await _context.producto_presentaciones
                .Where(p => p.id_producto == idProducto)
                .ToListAsync();
        }


        //buscar producto para agregarlo a la venta
        public async Task<List<ProductoVentaBuscarDTO>> BuscarProductosVenta(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<ProductoVentaBuscarDTO>();

            texto = texto.Trim();

            var productos = await _context.producto_presentaciones
                .Include(p => p.Producto)
                .Where(p =>
                    p.Producto.nombre.Contains(texto) ||
                    p.descripcion.Contains(texto) ||
                    p.Producto.codigo_barra.Contains(texto))
                .Select(p => new ProductoVentaBuscarDTO
                {
                    id_producto = p.id_producto,
                    id_producto_presentacion = p.id_producto_presentacion,
                    
                    nombre_producto = p.Producto.nombre,
                    presentacion = p.descripcion,
                    
                    unidades_equivalentes = p.unidades_equivalentes,
                    precio = p.precio,
                    stock = p.Producto.stock
                })
                .Take(10)
                .ToListAsync();

            return productos;
        }

        //Ingresar producto

        public async Task<Productos> CrearProducto(productocrear productoDto)
        {
            if (productoDto.presentaciones == null || productoDto.presentaciones.Count == 0)    
                throw new Exception("Debe ingresar al menos una presentación del producto.");

            bool existeCodigo = await _context.productos
                     .AnyAsync(p => p.codigo_barra == productoDto.codigo_barra);

            if (existeCodigo)
                throw new Exception("Ya existe un producto con ese código de barras.");

            var producto = new Productos
            {
                codigo_barra = productoDto.codigo_barra,
                nombre = productoDto.nombre,
                descripcion = productoDto.descripcion,
                id_categoria = productoDto.id_categoria,
                stock = productoDto.stock,
                precio_compra = productoDto.precio_compra,
                stock_minimo = productoDto.stock_minimo,
                impuesto = productoDto.impuesto,
                fecha_creacion = DateTime.Now
            };

            if (productoDto.stock <= 0)
                throw new Exception("El stock debe ser mayor a 0.");

            producto.costo_unitario = (productoDto.precio_compra / productoDto.stock);

            _context.productos.Add(producto);
            await _context.SaveChangesAsync();

            foreach (var item in productoDto.presentaciones)
            {
                var presentacion = new Producto_Presentacion
                {
                    id_producto = producto.id_producto,
                    descripcion = item.descripcion,
                    unidades_equivalentes = item.unidades_equivalentes,
                    precio = item.precio,
                    estado = true
                };

                _context.producto_presentaciones.Add(presentacion);
            }

            await _context.SaveChangesAsync();

            return producto;
        }


        //para subir imagenes
        public async Task<Productos?> SubirImagen(int id, IFormFile imagen)
        {
            var producto = await _context.productos.FindAsync(id);

            if (producto == null)
                return null;

            string nombreArchivo = $"{Guid.NewGuid()}_{imagen.FileName}";
            string rutaCarpeta = Path.Combine("wwwroot", "imagenes", "productos");
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            Directory.CreateDirectory(rutaCarpeta);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            producto.imagen = nombreArchivo;

            await _context.SaveChangesAsync();

            return producto;
        }

       
    }

}
