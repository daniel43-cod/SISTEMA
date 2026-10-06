using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.ProductoS;

public sealed class ProductoBuscarAdminService(SistemaDbContext context)
{
    public Task<List<ProductoSugerenciaDTO>> ListarNombres(CancellationToken cancellationToken = default) =>
        context.productos.AsNoTracking()
            .OrderBy(p => p.nombre).ThenBy(p => p.id_producto)
            .Select(p => new ProductoSugerenciaDTO { IdProducto = p.id_producto, Nombre = p.nombre })
            .ToListAsync(cancellationToken);

    public Task<List<ProductoSugerenciaDTO>> Buscar(BusquedaProductoAdminDTO filtro,
        CancellationToken cancellationToken = default)
    {
        if (filtro is null) throw new ValidationException("La búsqueda es obligatoria.");
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        var consulta = context.productos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.Trim();
            consulta = consulta.Where(p => p.nombre.StartsWith(nombre));
        }
        else
        {
            var codigo = filtro.CodigoBarra!.Trim();
            consulta = consulta.Where(p => p.codigo_barra == codigo);
        }
        return consulta.OrderBy(p => p.nombre).ThenBy(p => p.id_producto).Take(10)
            .Select(p => new ProductoSugerenciaDTO { IdProducto = p.id_producto, Nombre = p.nombre })
            .ToListAsync(cancellationToken);
    }
}
