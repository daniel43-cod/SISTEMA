using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.DTOs.Surtidos;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Services.Ventas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Surtidos;

public sealed class SurtidoService(SistemaDbContext db, CatalogoAuditoriaService auditoria,
    API_SISTEMA.Services.Productos.ProductoImagenService? imagenes = null)
{
    public async Task<ListaSurtidosDto> Listar(string? texto, int pagina, int tamano, CancellationToken ct, int? idMarca = null)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100 || (long)(pagina - 1) * tamano > int.MaxValue)
            throw new ValidationException("Revisa la página y el tamaño solicitado (1 a 100).");
        VentaReglas.Texto(texto, 200, "la búsqueda");
        var query = db.Surtidos.AsNoTracking();
        if (idMarca is <= 0) throw new ValidationException("La marca no es válida.");
        if (idMarca.HasValue) query = query.Where(s => s.IdMarca == idMarca.Value);
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var busqueda = texto.Trim().ToLowerInvariant();
            query = query.Where(s => s.Nombre.ToLower().Contains(busqueda));
        }
        var total = await query.CountAsync(ct);
        var elementos = await query.OrderBy(s => s.Nombre).ThenBy(s => s.IdSurtido)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(s => new SurtidoResumenDto(s.IdSurtido, s.Nombre, s.IdMarca, s.Marca.Nombre, s.Estado, s.Productos.Count, s.Imagen,
                db.Set<ProductoPresentacion>().Where(p => p.id_surtido == s.IdSurtido && p.estado && p.Presentacion.Estado == true)
                    .OrderBy(p => p.IdPresentacion)
                    .Select(p => new SurtidoTarjetaPresentacionDto(p.Presentacion.Descripcion, p.unidades_equivalentes, p.precio)).ToList()))
            .ToListAsync(ct);
        return new(total, pagina, tamano, elementos);
    }

    public Task<SurtidoDto> Crear(CrearSurtidoDto dto, CancellationToken ct, IFormFile? imagen = null)
        => GuardarConImagen(dto, ct, imagen);

    public Task<SurtidoDto> Actualizar(int id, ActualizarSurtidoDto dto, CancellationToken ct, IFormFile? imagen = null)
    {
        if (id <= 0) throw new ValidationException("El surtido no es válido.");
        return GuardarConImagen(dto, ct, imagen, id);
    }

    private async Task<SurtidoDto> GuardarConImagen(CrearSurtidoDto dto, CancellationToken ct, IFormFile? imagen, int? id = null)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var url = string.IsNullOrWhiteSpace(dto.UrlImagen) ? null
            : (imagenes ?? throw new InvalidOperationException("Servicio de imágenes no disponible.")).ValidarUrl(dto.UrlImagen);
        if (url is not null && imagen is not null)
            throw new ValidationException("Elige un enlace o un archivo, no ambos.");
        if (dto is ActualizarSurtidoDto { QuitarImagen: true } && (url is not null || imagen is not null))
            throw new ValidationException("No puedes quitar y reemplazar la imagen al mismo tiempo.");
        string? archivo = null;
        var commitIniciado = false;
        try
        {
            if (imagen is not null)
                url = archivo = await (imagenes ?? throw new InvalidOperationException("Servicio de imágenes no disponible.")).GuardarAsync(imagen, ct);
            return await GuardarConfiguracion(dto, url, ct, () => commitIniciado = true, id);
        }
        catch
        {
            if (archivo is not null && !commitIniciado) imagenes!.Eliminar(archivo);
            throw;
        }
    }

    private async Task<SurtidoDto> GuardarConfiguracion(CrearSurtidoDto dto, string? imagen, CancellationToken ct, Action antesCommit, int? id)
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
        var surtido = id.HasValue
            ? await db.Surtidos.Include(s => s.Productos).SingleOrDefaultAsync(s => s.IdSurtido == id.Value, ct)
                ?? throw new KeyNotFoundException("El surtido no existe.")
            : new Surtido();
        var existentes = id.HasValue
            ? await db.Set<ProductoPresentacion>().Where(p => p.id_surtido == id.Value).ToListAsync(ct)
            : new List<ProductoPresentacion>();
        Dictionary<string, object?> Valores() => new()
        {
            ["nombre"] = surtido.Nombre, ["id_marca"] = surtido.IdMarca, ["estado"] = surtido.Estado,
            ["imagen"] = surtido.Imagen,
            ["productos"] = string.Join(",", surtido.Productos.Select(p => p.IdProducto).Order()),
            ["presentaciones"] = System.Text.Json.JsonSerializer.Serialize(existentes.Where(p => p.estado)
                .OrderBy(p => p.IdPresentacion).Select(p => new { p.IdPresentacion, p.unidades_equivalentes, p.precio }))
        };
        var anteriores = id.HasValue ? Valores() : null;
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

        surtido.Nombre = dto.Nombre.Trim(); surtido.IdMarca = dto.IdMarca;
        surtido.Estado = dto is ActualizarSurtidoDto actualizacion ? actualizacion.Estado : true;
        if (dto is ActualizarSurtidoDto { QuitarImagen: true }) surtido.Imagen = null;
        else if (imagen is not null || !id.HasValue) surtido.Imagen = imagen;
        foreach (var componente in surtido.Productos.Where(p => !dto.IdsProductos.Contains(p.IdProducto)).ToArray())
        {
            db.SurtidoProductos.Remove(componente);
            surtido.Productos.Remove(componente);
        }
        foreach (var producto in productos)
            if (!surtido.Productos.Any(p => p.IdProducto == producto.IdProducto))
                surtido.Productos.Add(new SurtidoProducto { IdProducto = producto.IdProducto });
        if (!id.HasValue) db.Surtidos.Add(surtido);
        // Se conservan los identificadores; quitar una presentación solo la desactiva.
        foreach (var existente in existentes) existente.estado = false;
        foreach (var p in dto.Presentaciones)
        {
            var asociacion = existentes.SingleOrDefault(e => e.IdPresentacion == p.IdPresentacion);
            if (asociacion is null)
            {
                asociacion = new ProductoPresentacion { Surtido = surtido, IdPresentacion = p.IdPresentacion };
                db.Set<ProductoPresentacion>().Add(asociacion);
                existentes.Add(asociacion);
            }
            asociacion.unidades_equivalentes = p.UnidadesEquivalentes;
            asociacion.precio = p.Precio; asociacion.estado = true;
        }
        await db.SaveChangesAsync(ct);
        await auditoria.Agregar(id.HasValue ? "ACTUALIZAR" : "CREAR", "surtido", surtido.IdSurtido, anteriores, Valores(), ct);
        await db.SaveChangesAsync(ct);
        var respuesta = await Consultar(surtido.IdSurtido, ct);
        antesCommit();
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
            .Where(p => p.id_surtido == id && p.estado).OrderBy(p => p.IdPresentacion).ToListAsync(ct);
        decimal? Minimo(int unidades)
        {
            try { return SurtidoReglas.PrecioMinimoPresentacion(productos, unidades); }
            catch (ValidationException) { return null; }
        }
        return new(s.IdSurtido, s.Nombre, s.IdMarca, s.Marca.Nombre, s.Estado,
            productos.OrderBy(p => p.Nombre).Select(p => new SurtidoProductoDto(p.IdProducto, p.Nombre, p.PrecioPromedioCompra)).ToArray(),
            presentaciones.Select(p => new SurtidoPresentacionDto(p.id_producto_presentacion, p.IdPresentacion,
                p.Presentacion.Descripcion, p.unidades_equivalentes, p.precio, Minimo(p.unidades_equivalentes))).ToArray(), s.Imagen);
    }
}
