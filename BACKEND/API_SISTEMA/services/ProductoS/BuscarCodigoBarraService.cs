using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Productos;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Services.Productos
{
    public class BuscarCodigoBarraService
    {
        private readonly SistemaDbContext _context;

        public BuscarCodigoBarraService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<BuscarCodigoBarraDto?> BuscarPorCodigoBarra(
            string codigoBarra, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(codigoBarra))
            {
                throw new ValidationException(
                    "Debe ingresar un código de barras."
                );
            }

            var codigo = codigoBarra.Trim();
            if (codigo.Length > 100 || codigoBarra.Any(char.IsControl) || codigo.Any(char.IsWhiteSpace))
                throw new ValidationException("El código de barras admite hasta 100 caracteres, sin espacios internos ni caracteres de control.");

            var producto = await _context.productos
                .AsNoTracking()
                .Where(p => p.CodigoBarra == codigo)
                .Select(p => new
                {
                    marcaActiva = p.Marca.Estado,
                    categoriaActiva = p.Marca.Categoria.Estado,
                    costo = p.PrecioPromedioCompra,
                    id_producto = p.IdProducto,
                    codigo_barra = p.CodigoBarra,
                    nombre_producto = p.Nombre,
                    stock = p.Stock ?? 0,

                    presentaciones = p.ProductoPresentaciones
                        .Select(pr => new
                        {
                            activa = pr.estado && pr.Presentacion.Estado == true,
                            id_producto_presentacion =
                                pr.id_producto_presentacion,

                            presentacion =
                                pr.Presentacion.Descripcion,

                            unidades_equivalentes =
                                pr.unidades_equivalentes,

                            precio =
                                pr.precio
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (producto is null) return null;
            if (!producto.marcaActiva) throw new ValidationException("Marca desactivada.");
            if (!producto.categoriaActiva) throw new ValidationException("Categoría desactivada.");
            if (producto.stock <= 0) throw new ValidationException("Producto agotado.");
            if (producto.costo is not decimal costo || costo <= 0)
                throw new ValidationException("Sin costo promedio registrado.");

            var activas = producto.presentaciones.Where(p => p.activa).ToList();
            if (activas.Count == 0)
                throw new ValidationException(producto.presentaciones.Count == 0
                    ? "El producto no tiene presentaciones registradas." : "Presentación desactivada.");
            var validas = activas.Where(p => p.unidades_equivalentes > 0 && p.precio > 0 &&
                p.precio <= API_SISTEMA.Services.Ventas.VentaReglas.MontoMaximo && decimal.Round(p.precio, 2) == p.precio &&
                p.precio > costo * p.unidades_equivalentes).ToList();
            if (validas.Count == 0)
                throw new ValidationException("El producto no tiene presentaciones con precio válido mayor al costo promedio.");
            var disponibles = validas.Where(p => p.unidades_equivalentes <= producto.stock)
                .OrderBy(p => p.unidades_equivalentes).ThenBy(p => p.id_producto_presentacion).ToList();
            if (disponibles.Count == 0)
                throw new ValidationException("Existencias insuficientes para las presentaciones disponibles.");

            return new BuscarCodigoBarraDto
            {
                id_producto = producto.id_producto,
                codigo_barra = producto.codigo_barra ?? string.Empty,
                nombre_producto = producto.nombre_producto,
                stock = producto.stock,
                presentaciones = disponibles.Select(p => new PresentacionCodigoBarraDto
                {
                    id_producto_presentacion = p.id_producto_presentacion,
                    presentacion = p.presentacion ?? string.Empty,
                    unidades_equivalentes = p.unidades_equivalentes,
                    precio = p.precio
                }).ToList()
            };
        }

    }
}
