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
                    stock = p.Producto.stock??0
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
                    stock = p.Producto.stock??0
                })
                .Take(10)
                .ToListAsync();

            return productos;
        }

     

       
    }

}
