using API_SISTEMA.services.Auditoria;
using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.ProductoS;

public sealed class ProductoActualizarService(SistemaDbContext context, ProductoImagenService imagenes, ProductoAuditoriaService auditoria)
{
    public async Task<ProductoResumenDTO?> ActualizarProducto(int id, ActualizarProductoDTO dto,
        CancellationToken cancellationToken = default, IFormFile? imagen = null)
    {
        if (id <= 0) throw new ValidationException("El ID del producto debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("La información del producto es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var url = imagenes.ValidarUrl(dto.UrlImagen);
        if ((url != null && imagen != null) || (dto.QuitarImagen && (url != null || imagen != null)))
            throw new ValidationException("Elige un enlace, un archivo o quitar la imagen, no varias opciones.");
        if (dto.codigo_barra.Any(char.IsControl) || dto.codigo_barra.Trim().Any(char.IsWhiteSpace))
            throw new ValidationException("El código de barras no admite espacios internos ni caracteres de control.");
        if (dto.nombre.Any(char.IsControl))
            throw new ValidationException("El nombre no admite caracteres de control.");
        if (dto.presentaciones is not null)
        {
            foreach (var item in dto.presentaciones)
            {
                if (item is null) throw new ValidationException("Hay una presentación vacía.");
                Validator.ValidateObject(item, new ValidationContext(item), validateAllProperties: true);
                if (decimal.Round(item.precio, 2) != item.precio)
                    throw new ValidationException("El precio admite hasta dos decimales.");
            }
            if (dto.presentaciones.Select(p => p.id_presentacion).Distinct().Count() != dto.presentaciones.Count ||
                dto.presentaciones.Where(p => p.id_producto_presentacion.HasValue)
                    .GroupBy(p => p.id_producto_presentacion).Any(g => g.Count() > 1))
                throw new ValidationException("No puedes repetir una presentación ni su ID.");
        }

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
        var anteriores = ProductoAuditoriaService.Datos(producto);
        var cambiosPresentaciones = new List<(API_SISTEMA.models.Producto_Presentacion Entidad, Dictionary<string, object?>? Anteriores)>();

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

        if (dto.presentaciones is not null)
        {
            var existentes = await context.producto_presentaciones.Where(p => p.id_producto == id)
                .ToListAsync(cancellationToken);
            cambiosPresentaciones.AddRange(existentes.Select(p => (p, (Dictionary<string, object?>?)ProductoAuditoriaService.DatosPresentacion(p))));
            var ids = dto.presentaciones.Select(p => p.id_presentacion).ToList();
            var catalogo = await context.presentaciones.AsNoTracking().Where(p => ids.Contains(p.IdPresentacion))
                .ToDictionaryAsync(p => p.IdPresentacion, cancellationToken);
            // Validar toda la colección antes de modificar entidades rastreadas.
            foreach (var item in dto.presentaciones)
            {
                if (!catalogo.TryGetValue(item.id_presentacion, out var definicion) ||
                    (item.estado == true && definicion.Estado != true))
                    throw new ValidationException("La presentación debe existir y estar activa para habilitarla.");
                if (item.id_producto_presentacion.HasValue)
                {
                    var existente = existentes.SingleOrDefault(p => p.id_producto_presentacion == item.id_producto_presentacion.Value);
                    if (existente is null)
                        throw new ValidationException("La presentación indicada no pertenece a este producto.");
                    if (existente.IdPresentacion != item.id_presentacion)
                        throw new ValidationException("No se puede cambiar el tipo de una presentación existente. Desactívala y agrega otra.");
                }
                else if (existentes.Any(p => p.IdPresentacion == item.id_presentacion))
                    throw new ValidationException("Esta presentación ya está asociada al producto. Usa su ID para editarla o reactivarla.");
            }
            foreach (var existente in existentes)
                if (!dto.presentaciones.Any(p => p.id_producto_presentacion == existente.id_producto_presentacion))
                    existente.estado = false;
            foreach (var item in dto.presentaciones)
            {
                var asociacion = item.id_producto_presentacion.HasValue
                    ? existentes.Single(p => p.id_producto_presentacion == item.id_producto_presentacion.Value)
                    : new API_SISTEMA.models.Producto_Presentacion { id_producto = id, IdPresentacion = item.id_presentacion };
                asociacion.precio = item.precio;
                asociacion.unidades_equivalentes = item.unidades_equivalentes;
                asociacion.estado = item.estado!.Value;
                if (!item.id_producto_presentacion.HasValue)
                {
                    context.producto_presentaciones.Add(asociacion);
                    cambiosPresentaciones.Add((asociacion, null));
                }
            }
        }

        producto.codigo_barra = codigo;
        producto.nombre = nombre;
        producto.IdMarca = marca.IdMarca;
        producto.stock_minimo = dto.stock_minimo!.Value;
        string? archivo = null;
        var commitIniciado = false;
        try
        {
            if (imagen != null) url = archivo = await imagenes.GuardarAsync(imagen, cancellationToken);
            if (dto.QuitarImagen) producto.imagen = null;
            else if (url != null) producto.imagen = url;
            await context.SaveChangesAsync(cancellationToken);
            await auditoria.Registrar(producto, anteriores, cancellationToken);
            foreach (var cambio in cambiosPresentaciones)
                await auditoria.RegistrarPresentacion(cambio.Entidad, cambio.Anteriores, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            commitIniciado = true;
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // Conservar el archivo si el resultado del commit es incierto.
            if (archivo != null && !commitIniciado) imagenes.Eliminar(archivo);
            throw;
        }
        return new ProductoResumenDTO
        {
            IdProducto = producto.id_producto, CodigoBarra = producto.codigo_barra, Nombre = producto.nombre,
            Imagen = producto.imagen, StockUnidades = producto.stock ?? 0, StockMinimo = producto.stock_minimo ?? 0,
            IdMarca = marca.IdMarca, Marca = marca.Nombre, MarcaActiva = true,
            IdCategoria = marca.IdCategoria, Categoria = marca.Categoria, CategoriaActiva = true
        };
    }
}
