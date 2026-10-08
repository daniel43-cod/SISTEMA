using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Productos;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Services
{
    public class ProductoService
    {
        private readonly SistemaDbContext _context;

        public ProductoService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<ProductoPaginaDto> ListarAdministracion(ProductoConsultaDto filtro,
            CancellationToken cancellationToken)
        {
            Validator.ValidateObject(filtro, new ValidationContext(filtro), validateAllProperties: true);
            var consulta = _context.productos.AsNoTracking().AsQueryable();
            var texto = filtro.Texto?.Trim();
            if (!string.IsNullOrEmpty(texto))
                consulta = consulta.Where(p => p.nombre.Contains(texto) ||
                    (p.codigo_barra != null && p.codigo_barra.Contains(texto)));
            if (filtro.IdMarca.HasValue)
                consulta = consulta.Where(p => p.IdMarca == filtro.IdMarca.Value);
            if (filtro.IdCategoria.HasValue)
                consulta = consulta.Where(p => p.Marca.IdCategoria == filtro.IdCategoria.Value);

            var total = await consulta.CountAsync(cancellationToken);
            var items = await consulta.OrderBy(p => p.nombre).ThenBy(p => p.id_producto)
                .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
                .Select(p => new ProductoResumenDto
                {
                    IdProducto = p.id_producto, CodigoBarra = p.codigo_barra, Nombre = p.nombre,
                    Imagen = p.imagen, StockUnidades = p.stock ?? 0, StockMinimo = p.stock_minimo ?? 0,
                    IdMarca = p.IdMarca, Marca = p.Marca.Nombre, MarcaActiva = p.Marca.Estado,
                    IdCategoria = p.Marca.IdCategoria, Categoria = p.Marca.Categoria.nombreCategoria,
                    CategoriaActiva = p.Marca.Categoria.Estado
                }).ToListAsync(cancellationToken);
            return new ProductoPaginaDto
            {
                Pagina = filtro.Pagina, TamanoPagina = filtro.TamanoPagina, Total = total, Items = items
            };
        }

        public async Task<ProductoDetalleDto?> ObtenerDetalleAdministracion(int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0) throw new ValidationException("El ID del producto debe ser mayor que cero.");
            var detalle = await _context.productos.AsNoTracking().Where(p => p.id_producto == id)
                .Select(p => new ProductoDetalleDto
                {
                    IdProducto = p.id_producto, CodigoBarra = p.codigo_barra, Nombre = p.nombre,
                    Imagen = p.imagen, StockUnidades = p.stock ?? 0, StockMinimo = p.stock_minimo ?? 0,
                    IdMarca = p.IdMarca, Marca = p.Marca.Nombre, MarcaActiva = p.Marca.Estado,
                    IdCategoria = p.Marca.IdCategoria, Categoria = p.Marca.Categoria.nombreCategoria,
                    CategoriaActiva = p.Marca.Categoria.Estado, FechaCreacion = p.fecha_creacion
                }).SingleOrDefaultAsync(cancellationToken);
            if (detalle is null) return null;

            detalle.Presentaciones = await _context.producto_presentaciones.AsNoTracking()
                .Where(pp => pp.id_producto == id)
                .OrderBy(pp => pp.Presentacion.Descripcion).ThenBy(pp => pp.id_producto_presentacion)
                .Select(pp => new ProductoPresentacionDetalleDto
                {
                    IdProductoPresentacion = pp.id_producto_presentacion,
                    IdPresentacion = pp.IdPresentacion, Descripcion = pp.Presentacion.Descripcion,
                    UnidadesEquivalentes = pp.unidades_equivalentes, Precio = pp.precio,
                    Activa = pp.estado, PresentacionActiva = pp.Presentacion.Estado == true
                }).ToListAsync(cancellationToken);
            foreach (var presentacion in detalle.Presentaciones)
                presentacion.PresentacionesDisponibles = presentacion.Activa && presentacion.PresentacionActiva &&
                    detalle.MarcaActiva && detalle.CategoriaActiva &&
                    presentacion.UnidadesEquivalentes > 0 && detalle.StockUnidades > 0
                        ? detalle.StockUnidades / presentacion.UnidadesEquivalentes : 0;
            return detalle;
        }
        public async Task<List<ListarPresentacionProductoDto>> ListarPresentaciones(int idProducto)
        {
            return await _context.producto_presentaciones
                .Where(p => p.id_producto == idProducto)
                .Select(p => new ListarPresentacionProductoDto
                {
                    id_producto_presentacion = p.id_producto_presentacion,
                    id_producto = p.id_producto,
                    id_presentacion = p.IdPresentacion,
                    descripcion = p.Presentacion.Descripcion,
                    unidades_equivalentes = p.unidades_equivalentes,
                    precio = p.precio,
                    estado = p.estado
                })
                .ToListAsync();
        }


        //buscar producto para agregarlo a la venta
        public async Task<List<ProductoVentaBuscarDto>> BuscarProductosVenta(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<ProductoVentaBuscarDto>();

            texto = texto.Trim();

            var productos = await _context.producto_presentaciones
                .Include(p => p.Producto)
                .Where(p =>
                    p.Producto.nombre.Contains(texto) ||
                    (p.Presentacion.Descripcion != null && p.Presentacion.Descripcion.Contains(texto)) ||
                    p.Producto.codigo_barra.Contains(texto))
                .Select(p => new ProductoVentaBuscarDto
                {
                    id_producto = p.id_producto,
                    id_producto_presentacion = p.id_producto_presentacion,
                    
                    nombre_producto = p.Producto.nombre,
                    presentacion = p.Presentacion.Descripcion,
                    
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
