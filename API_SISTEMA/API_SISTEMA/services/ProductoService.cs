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

        public async Task<List<Productos>> ListarProductos()
        {
            return await _context.productos.ToListAsync();
        }

        public async Task<List<Producto_Presentacion>> ListarPresentaciones(int idProducto)
        {
            return await _context.producto_presentaciones
                .Where(p => p.id_producto == idProducto)
                .ToListAsync();
        }

        public async Task<List<Productos>> BuscarPorNombre(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return new List<Productos>();
            }

            var textoLower = texto.ToLower();

            return await _context.productos
                .Where(p => p.nombre.ToLower().Contains(textoLower))
                .ToListAsync();
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

        public async Task<List<ProductoBuscarDTOs>> BuscarProductos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<ProductoBuscarDTOs>();

            texto = texto.Trim();

            var productos = await _context.productos
                .Where(p =>
                    p.nombre.Contains(texto) ||
                    p.codigo_barra.Contains(texto))
                .Select(p => new ProductoBuscarDTOs
                {
                    id_producto = p.id_producto,
                    codigo_barra = p.codigo_barra,
                    nombre = p.nombre
                })
                .Take(10)
                .ToListAsync();

            return productos;
        }


    }

}
