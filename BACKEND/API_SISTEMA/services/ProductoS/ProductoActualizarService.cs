using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.ProductoS;

public sealed class ProductoActualizarService(SistemaDbContext context)
{
    public async Task<ProductoResumenDTO?> ActualizarProducto(int id, ActualizarProductoDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("El ID del producto debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("La información del producto es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        if (dto.codigo_barra.Any(char.IsControl) || dto.codigo_barra.Trim().Any(char.IsWhiteSpace))
            throw new ValidationException("El código de barras no admite espacios internos ni caracteres de control.");
        if (dto.nombre.Any(char.IsControl))
            throw new ValidationException("El nombre no admite caracteres de control.");

        var codigo = dto.codigo_barra.Trim();
        var nombre = dto.nombre.Trim();
        //convierte el codigo a mayuscula para compara duplicados
        var codigoNormalizado = codigo.ToUpperInvariant();
        var nombreNormalizado = nombre.ToUpperInvariant();
        // Serializa comprobación y escritura, también con las altas de productos.
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var producto = await context.productos.SingleOrDefaultAsync(p => p.id_producto == id, cancellationToken);
        if (producto is null) return null;

        if (await context.productos.AnyAsync(p => p.id_producto != id && p.codigo_barra != null &&
            p.codigo_barra.Trim().ToUpper() == codigoNormalizado, cancellationToken))
            throw new ProductoDuplicadoException();
        if (await context.productos.AnyAsync(p => p.id_producto != id &&
            p.nombre.Trim().ToUpper() == nombreNormalizado, cancellationToken))
            throw new ProductoDuplicadoException("Ya existe un producto con ese nombre.");

        var marca = await context.Marcas.AsNoTracking().Where(m => m.IdMarca == dto.IdMarca &&
            m.Estado && m.Categoria.Estado).Select(m => new
            {
                m.IdMarca, m.Nombre, m.IdCategoria, Categoria = m.Categoria.nombreCategoria
            }).SingleOrDefaultAsync(cancellationToken);
        if (marca is null) throw new ValidationException("La marca y su categoría deben existir y estar activas.");

        producto.codigo_barra = codigo;
        producto.nombre = nombre;
        producto.IdMarca = marca.IdMarca;
        producto.stock_minimo = dto.stock_minimo!.Value;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ProductoResumenDTO
        {
            IdProducto = producto.id_producto, CodigoBarra = producto.codigo_barra, Nombre = producto.nombre,
            Imagen = producto.imagen, StockUnidades = producto.stock ?? 0, StockMinimo = producto.stock_minimo ?? 0,
            IdMarca = marca.IdMarca, Marca = marca.Nombre, MarcaActiva = true,
            IdCategoria = marca.IdCategoria, Categoria = marca.Categoria, CategoriaActiva = true
        };
    }
}
