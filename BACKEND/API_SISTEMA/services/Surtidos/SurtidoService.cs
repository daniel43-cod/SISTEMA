using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.DTOs.Surtidos;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Services.Ventas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Surtidos;

public sealed class SurtidoService(SistemaDbContext db, CatalogoAuditoriaService auditoria)
{
    public async Task<ListaSurtidosDto> Listar(string? texto, int pagina, int tamano, CancellationToken ct)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100 || (long)(pagina - 1) * tamano > int.MaxValue)
            throw new ValidationException("Revisa la página y el tamaño solicitado (1 a 100).");
        VentaReglas.Texto(texto, 200, "la búsqueda");
        var query = db.Surtidos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var busqueda = texto.Trim().ToLowerInvariant();
            query = query.Where(s => s.Nombre.ToLower().Contains(busqueda));
        }
        var total = await query.CountAsync(ct);
        var elementos = await query.OrderBy(s => s.Nombre).ThenBy(s => s.IdSurtido)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(s => new SurtidoResumenDto(s.IdSurtido, s.Nombre, s.IdMarca, s.Marca.Nombre, s.Estado, s.Productos.Count))
            .ToListAsync(ct);
        return new(total, pagina, tamano, elementos);
    }

    public async Task<SurtidoDto> Crear(CrearSurtidoDto dto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        VentaReglas.Texto(dto.Nombre, 200, "el nombre del surtido", true);
        if (dto.IdsProductos.Any(id => id <= 0) || dto.IdsProductos.Distinct().Count() != dto.IdsProductos.Count)
            throw new ValidationException("Selecciona productos válidos sin repetirlos.");
        if (dto.Presentaciones.Any(p => p is null))
            throw new ValidationException("Revisa las presentaciones del surtido.");
        foreach (var p in dto.Presentaciones)
            Validator.ValidateObject(p, new ValidationContext(p), true);
        if (dto.Presentaciones.Select(p => p.IdPresentacion).Distinct().Count() != dto.Presentaciones.Count)
            throw new ValidationException("No repitas una presentación dentro del surtido.");

        // La configuración completa y su auditoría se confirman juntas.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var marca = await db.Marcas.Include(m => m.Categoria).SingleOrDefaultAsync(m => m.IdMarca == dto.IdMarca, ct);
        if (marca is null || !marca.Estado || !marca.Categoria.Estado)
            throw new ValidationException("Selecciona una marca y categoría activas para el surtido.");
        var productos = await db.Set<Models.Productos>().Include(p => p.Marca).ThenInclude(m => m.Categoria)
            .Where(p => dto.IdsProductos.Contains(p.IdProducto)).ToListAsync(ct);
        if (productos.Count != dto.IdsProductos.Count)
            throw new ValidationException("Uno o más productos seleccionados no existen.");
        if (productos.Any(p => !p.Marca.Estado || !p.Marca.Categoria.Estado))
            throw new ValidationException("Los productos deben pertenecer a marcas y categorías activas.");
        SurtidoReglas.ValidarCostosPromedio(productos);
        var idsPresentaciones = dto.Presentaciones.Select(p => p.IdPresentacion).ToArray();
        var disponibles = await db.presentaciones.CountAsync(p => idsPresentaciones.Contains(p.IdPresentacion) && p.Estado == true, ct);
        if (disponibles != idsPresentaciones.Length)
            throw new ValidationException("Selecciona presentaciones existentes y activas.");
        foreach (var p in dto.Presentaciones)
            SurtidoReglas.ValidarPrecioPresentacion(productos, p.UnidadesEquivalentes, p.Precio);

        var surtido = new Surtido { Nombre = dto.Nombre.Trim(), IdMarca = dto.IdMarca, Estado = true };
        foreach (var producto in productos)
            surtido.Productos.Add(new SurtidoProducto { IdProducto = producto.IdProducto });
        db.Surtidos.Add(surtido);
        db.Set<ProductoPresentacion>().AddRange(dto.Presentaciones.Select(p => new ProductoPresentacion
        {
            Surtido = surtido, id_producto = null, IdPresentacion = p.IdPresentacion,
            unidades_equivalentes = p.UnidadesEquivalentes, precio = p.Precio, estado = true
        }));
        await db.SaveChangesAsync(ct);
        await auditoria.Agregar("CREAR", "surtido", surtido.IdSurtido, null, new()
        {
            ["nombre"] = surtido.Nombre, ["id_marca"] = surtido.IdMarca,
            ["productos"] = string.Join(",", dto.IdsProductos.Order()),
            ["presentaciones"] = System.Text.Json.JsonSerializer.Serialize(dto.Presentaciones)
        }, ct);
        await db.SaveChangesAsync(ct);
        var respuesta = await Consultar(surtido.IdSurtido, ct);
        await tx.CommitAsync(ct);
        return respuesta;
    }

    public async Task<SurtidoDto> Consultar(int id, CancellationToken ct)
    {
        var s = await db.Surtidos.AsNoTracking().Include(s => s.Marca)
            .Include(s => s.Productos).ThenInclude(p => p.Producto)
            .SingleOrDefaultAsync(s => s.IdSurtido == id, ct)
            ?? throw new KeyNotFoundException("El surtido no existe.");
        var productos = s.Productos.Select(p => p.Producto).ToArray();
        var presentaciones = await db.Set<ProductoPresentacion>().AsNoTracking().Include(p => p.Presentacion)
            .Where(p => p.id_surtido == id).OrderBy(p => p.IdPresentacion).ToListAsync(ct);
        decimal? Minimo(int unidades)
        {
            try { return SurtidoReglas.PrecioMinimoPresentacion(productos, unidades); }
            catch (ValidationException) { return null; }
        }
        return new(s.IdSurtido, s.Nombre, s.IdMarca, s.Marca.Nombre, s.Estado,
            productos.OrderBy(p => p.Nombre).Select(p => new SurtidoProductoDto(p.IdProducto, p.Nombre, p.PrecioPromedioCompra)).ToArray(),
            presentaciones.Select(p => new SurtidoPresentacionDto(p.id_producto_presentacion, p.IdPresentacion,
                p.Presentacion.Descripcion, p.unidades_equivalentes, p.precio, Minimo(p.unidades_equivalentes))).ToArray());
    }
}
