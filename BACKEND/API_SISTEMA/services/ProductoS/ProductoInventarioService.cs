using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Productos;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Productos;

public sealed class ProductoInventarioService(SistemaDbContext context)
{
    public async Task<ProductoInventarioPaginaDto> Listar(ProductoConsultaDto filtro,
        CancellationToken cancellationToken = default)
    {
        if (filtro is null) throw new ValidationException("Los filtros son obligatorios.");
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);

        var consulta = context.productos.AsNoTracking();
        var texto = filtro.Texto?.Trim();
        if (!string.IsNullOrEmpty(texto))
            consulta = consulta.Where(p => p.Nombre.Contains(texto) ||
                (p.CodigoBarra != null && p.CodigoBarra.Contains(texto)));
        if (filtro.IdMarca.HasValue)
            consulta = consulta.Where(p => p.IdMarca == filtro.IdMarca.Value);
        if (filtro.IdCategoria.HasValue)
            consulta = consulta.Where(p => p.Marca.IdCategoria == filtro.IdCategoria.Value);

        var total = await consulta.CountAsync(cancellationToken);
        var items = await consulta.OrderBy(p => p.Nombre).ThenBy(p => p.IdProducto)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(p => new ProductoInventarioDto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                PrecioPromedioCompra = p.PrecioPromedioCompra,
                Stock = p.Stock ?? 0,
                CodigoBarra = p.CodigoBarra,
                IdMarca = p.IdMarca,
                Marca = p.Marca.Nombre
            }).ToListAsync(cancellationToken);

        return new ProductoInventarioPaginaDto
        {
            Pagina = filtro.Pagina, TamanoPagina = filtro.TamanoPagina,
            Total = total, Items = items
        };
    }
}
