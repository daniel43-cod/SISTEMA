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
        public async Task<Productos> CrearProducto(productocrear productoDto, IFormFile? imagen)
        {
            var producto = new Productos
            {
                codigo_barra = productoDto.codigo_barra,
                nombre = productoDto.nombre,
                descripcion = productoDto.descripcion,
                id_categoria = productoDto.id_categoria,
                precio_compra = productoDto.precio_compra,
                stock = productoDto.stock,
                stock_minimo = productoDto.stock_minimo,
                impuesto = productoDto.impuesto,
                fecha_creacion = DateTime.Now
            };

            if (imagen != null && imagen.Length > 0)
            {
                string nombreArchivo = $"{Guid.NewGuid()}_{imagen.FileName}";
                string rutaCarpeta = Path.Combine("wwwroot", "imagenes", "productos");
                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                Directory.CreateDirectory(rutaCarpeta);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                producto.imagen = nombreArchivo;
            }


            _context.productos.Add(producto);
            await _context.SaveChangesAsync();

            if (productoDto.precios == null)
            {
                throw new Exception("La lista es NULL");
            }

            if (productoDto.precios.Count == 0)
            {
                throw new Exception("La lista está vacía");
            }

            foreach (var item in productoDto.precios)
            {
                var precio = new Producto_precio
                {
                    id_producto = producto.id_producto,
                    id_tipo_cliente = item.id_tipo_cliente,
                    precio = item.precio,
                    estado = true
                };

                _context.producto_precios.Add(precio);
            }

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
