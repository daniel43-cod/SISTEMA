using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Marcas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Marca;

public class MarcaActualizarService
{
    private readonly SistemaDbContext _context;
    private readonly MarcaImagenService _imagenes;

    public MarcaActualizarService(SistemaDbContext context, MarcaImagenService imagenes)
    {
        _context = context;
        _imagenes = imagenes;
    }

    public async Task<RespuestaMarcaDTO?> ActualizarMarca(int idMarca, ActualizarMarcaDTO dto,
        CancellationToken cancellationToken = default, IFormFile? imagen = null)
    {
        if (idMarca <= 0) throw new ValidationException("El ID de la marca debe ser mayor que cero.");
        if (dto is null)
            throw new ValidationException("La información de la marca es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var nombre = dto.Nombre.Trim();
        var normalizado = nombre.ToUpperInvariant();

        var url = _imagenes.ValidarUrl(dto.UrlImagen);
        if (url != null && imagen != null)
            throw new ValidationException("Elige un enlace o un archivo, no ambos.");
        if (dto.QuitarImagen && (url != null || imagen != null))
            throw new ValidationException("No puedes quitar y reemplazar la imagen al mismo tiempo.");
        if (!await _context.Marcas.AnyAsync(m => m.IdMarca == idMarca, cancellationToken)) return null;
        string? archivoGuardado = null;
        var commitIniciado = false;
        try
        {
        if (imagen != null) url = archivoGuardado = await _imagenes.GuardarAsync(imagen, cancellationToken);

        // Mantiene las validaciones y la creación dentro de la misma transacción.
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var marca = await _context.Marcas.SingleOrDefaultAsync(m => m.IdMarca == idMarca, cancellationToken);
        if (marca is null)
        {
            if (archivoGuardado != null) _imagenes.Eliminar(archivoGuardado);
            return null;
        }

        var categoriaActiva = await _context.categorias.AnyAsync(
            c => c.IdCategoria == dto.IdCategoria && c.Estado, cancellationToken);
        if (!categoriaActiva)
            throw new ValidationException("La categoría no existe o está inactiva.");

        // La marca es única por nombre, incluso en otra categoría o estando inactiva.
        var duplicada = await _context.Marcas.AnyAsync(
            m => m.IdMarca != idMarca && m.Nombre.Trim().ToUpper() == normalizado, cancellationToken);
        if (duplicada) throw new MarcaDuplicadaException();

        // Conserva estado e imagen si no se solicitó un cambio explícito.
        marca.Nombre = nombre;
        marca.IdCategoria = dto.IdCategoria;
        if (dto.QuitarImagen) marca.UrlImagen = null;
        else if (url != null) marca.UrlImagen = url;
        await _context.SaveChangesAsync(cancellationToken);
        commitIniciado = true;
        await transaction.CommitAsync(cancellationToken);

        return new RespuestaMarcaDTO
        {
            IdMarca = marca.IdMarca,
            Nombre = marca.Nombre,
            UrlImagen = marca.UrlImagen,
            IdCategoria = marca.IdCategoria,
            Estado = marca.Estado
        };
        }
        catch
        {
            if (archivoGuardado != null && !commitIniciado) _imagenes.Eliminar(archivoGuardado);
            throw;
        }
    }
}

