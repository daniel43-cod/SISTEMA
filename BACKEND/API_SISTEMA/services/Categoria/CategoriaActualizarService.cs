using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Categoria;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Categoria;

public class CategoriaActualizarService
{
    private readonly SistemaDbContext _context;
    private readonly CategoriaImagenService _imagenes;

    public CategoriaActualizarService(SistemaDbContext context, CategoriaImagenService imagenes)
    {
        _context = context;
        _imagenes = imagenes;
    }

    // Devuelve null si no existe la categoría; el controlador podrá responder 404.
    public async Task<RespuestaCategoriaDTO?> ActualizarCategoria(
        int idCategoria, ActualizarCategoriaDTO dto,
        CancellationToken cancellationToken = default, IFormFile? imagen = null)
    {
        if (idCategoria <= 0)
            throw new ValidationException("El ID de la categoría debe ser mayor que cero.");
        if (dto is null)
            throw new ValidationException("La información de la categoría es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

        var nombre = dto.Nombre.Trim();
        var normalizado = nombre.ToUpperInvariant();
        var url = _imagenes.ValidarUrl(dto.UrlImagen);
        if (url != null && imagen != null)
            throw new ValidationException("Elige un enlace o un archivo, no ambos.");
        if (dto.QuitarImagen && (url != null || imagen != null))
            throw new ValidationException("No puedes quitar y reemplazar la imagen al mismo tiempo.");

        // No procesar un archivo para un ID inexistente.
        if (!await _context.categorias.AnyAsync(c => c.IdCategoria == idCategoria, cancellationToken))
            return null;

        string? archivoGuardado = null;
        bool commitIniciado = false;
        try
        {
            // Procesar antes de abrir la transacción evita mantener bloqueos durante la conversión.
            if (imagen != null)
                url = archivoGuardado = await _imagenes.GuardarAsync(imagen, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            var categoria = await _context.categorias.SingleOrDefaultAsync(
                c => c.IdCategoria == idCategoria, cancellationToken);
            if (categoria is null)
            {
                if (archivoGuardado != null) _imagenes.Eliminar(archivoGuardado);
                return null;
            }

            // Excluir el propio registro permite guardar sin cambiar el nombre.
            // También se comprueban las categorías inactivas.
            var duplicada = await _context.categorias.AnyAsync(c =>
                c.IdCategoria != idCategoria && c.nombreCategoria != null &&
                c.nombreCategoria.Trim().ToUpper() == normalizado, cancellationToken);
            if (duplicada) throw new CategoriaDuplicadaException();

            categoria.nombreCategoria = nombre;
            if (dto.QuitarImagen) categoria.UrlImagen = null;
            else if (url != null) categoria.UrlImagen = url;
            // Estado y fecha de creación se mantienen.
            await _context.SaveChangesAsync(cancellationToken);
            // Una confirmación fallida puede tener resultado incierto; conservar el archivo.
            commitIniciado = true;
            await transaction.CommitAsync(cancellationToken);

            // No borrar la imagen anterior: podría estar referenciada por otro registro.
            return new RespuestaCategoriaDTO
            {
                IdCategoria = categoria.IdCategoria,
                Nombre = categoria.nombreCategoria,
                Estado = categoria.Estado,
                UrlImagen = categoria.UrlImagen
            };
        }
        catch
        {
            if (archivoGuardado != null && !commitIniciado) _imagenes.Eliminar(archivoGuardado);
            throw;
        }
    }
}
