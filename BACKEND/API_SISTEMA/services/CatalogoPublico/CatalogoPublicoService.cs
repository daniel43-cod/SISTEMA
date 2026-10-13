using API_SISTEMA.Data;
using API_SISTEMA.Dtos.CatalogoPublico;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.CatalogoPublico;

public sealed class CatalogoPublicoService(SistemaDbContext context)
{
    public async Task<ProductosPublicosPaginaDto?> ListarProductosPorMarca(
        int idMarca, int pagina = 1, int tamanoPagina = 24, CancellationToken cancellationToken = default)
    {
        if (idMarca <= 0) throw new ArgumentOutOfRangeException(nameof(idMarca));
        if (pagina < 1 || pagina > 100000) throw new ArgumentOutOfRangeException(nameof(pagina));
        if (tamanoPagina < 1 || tamanoPagina > 60) throw new ArgumentOutOfRangeException(nameof(tamanoPagina));

        if (!await context.Marcas.AnyAsync(m => m.IdMarca == idMarca && m.Estado && m.Categoria.Estado,
            cancellationToken)) return null;

        var productos = await context.productos.AsNoTracking()
            .Where(p => p.IdMarca == idMarca && p.Marca.Estado && p.Marca.Categoria.Estado &&
                p.ProductoPresentaciones.Any(pp => pp.estado && pp.Presentacion.Estado == true &&
                    pp.unidades_equivalentes > 0 && pp.precio >= 0))
            .OrderBy(p => p.nombre).ThenBy(p => p.id_producto)
            .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina + 1)
            .Select(p => new ProductoPublicoDto
            {
                IdProducto = p.id_producto,
                Nombre = p.nombre,
                UrlImagen = p.imagen,
                Presentaciones = p.ProductoPresentaciones
                    .Where(pp => pp.estado && pp.Presentacion.Estado == true &&
                        pp.unidades_equivalentes > 0 && pp.precio >= 0)
                    .OrderBy(pp => pp.unidades_equivalentes).ThenBy(pp => pp.id_producto_presentacion)
                    .Select(pp => new PresentacionPublicaDto
                    {
                        IdProductoPresentacion = pp.id_producto_presentacion,
                        Nombre = pp.Presentacion.Descripcion ?? "Presentación",
                        Precio = pp.precio,
                        Disponible = (pp.Producto.stock ?? 0) >= pp.unidades_equivalentes
                    }).ToList()
            }).ToListAsync(cancellationToken);

        return new ProductosPublicosPaginaDto
        {
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            HayMas = productos.Count > tamanoPagina,
            Productos = productos.Take(tamanoPagina).ToList()
        };
    }

    // null indica categoría inexistente o inactiva; una lista vacía indica que no tiene marcas activas.
    public async Task<List<MarcaPublicaDto>?> ListarMarcasPorCategoria(int idCategoria, CancellationToken cancellationToken = default)
    {
        if (idCategoria <= 0)throw new ArgumentOutOfRangeException(nameof(idCategoria));

        var categoriaActiva = await context.categorias.AsNoTracking().AnyAsync/*verifica si existe una coindicencia*/(c => c.IdCategoria == idCategoria && c.Estado, cancellationToken);

        if (!categoriaActiva) return null;

        return await context.Marcas.AsNoTracking()
            .Where(m => m.IdCategoria == idCategoria && m.Estado && m.Categoria.Estado)
            .OrderBy(m => m.Nombre).ThenBy(m => m.IdMarca)
            .Select(m => new MarcaPublicaDto
            {
                IdMarca = m.IdMarca,
                Nombre = m.Nombre,
                UrlImagen = m.UrlImagen
            })
            .ToListAsync(cancellationToken);
    }

    // La visibilidad usa el estado actual hasta incorporar la administración pública.
    public Task<List<CategoriaPublicaDto>> ListarCategorias(CancellationToken cancellationToken = default)
        => context.categorias.AsNoTracking().Where(c => c.Estado)
            .OrderBy(c => c.nombreCategoria).ThenBy(c => c.IdCategoria).Select(c => new CategoriaPublicaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.nombreCategoria,
                UrlImagen = c.UrlImagen
            })
            .ToListAsync(cancellationToken);
}
