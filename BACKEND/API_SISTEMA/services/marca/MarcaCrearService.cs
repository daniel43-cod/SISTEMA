using API_SISTEMA.Services.Auditoria;
using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Marcas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Marca;

public class MarcaCrearService
{
    private readonly CatalogoAuditoriaService _auditoria;
    private readonly SistemaDbContext _context;
    private readonly MarcaImagenService _imagenes;

    public MarcaCrearService(SistemaDbContext context, MarcaImagenService imagenes, CatalogoAuditoriaService auditoria)
    {
        _auditoria = auditoria;
        _context = context;
        _imagenes = imagenes;
    }

    public async Task<RespuestaMarcaDto> CrearMarca(CrearMarcaDto dto,
        CancellationToken cancellationToken = default, IFormFile? imagen = null)
    {
        if (dto is null)
            throw new ValidationException("La información de la marca es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var nombre = dto.Nombre.Trim();
        var normalizado = nombre.ToUpperInvariant();

        var url = _imagenes.ValidarUrl(dto.UrlImagen);
        if (url != null && imagen != null)
            throw new ValidationException("Elige un enlace o un archivo, no ambos.");
        string? archivoGuardado = null;
        var commitIniciado = false;
        try
        {

            // Mantiene las validaciones y la creación dentro de la misma transacción.
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            var categoriaActiva = await _context.categorias.AnyAsync(
                c => c.IdCategoria == dto.IdCategoria && c.Estado, cancellationToken);
            if (!categoriaActiva)
                throw new ValidationException("La categoría no existe o está inactiva.");

            // La marca es única por nombre, incluso en otra categoría o estando inactiva.
            var duplicada = await _context.Marcas.AnyAsync(
                m => m.Nombre.Trim().ToUpper() == normalizado, cancellationToken);
            if (duplicada) throw new MarcaDuplicadaException();

            if (imagen != null)
                url = archivoGuardado = await _imagenes.GuardarAsync(imagen, cancellationToken);

            var marca = new API_SISTEMA.Models.Marca
            {
                Nombre = nombre,
                IdCategoria = dto.IdCategoria,
                UrlImagen = url,
                Estado = true // El cliente no elige el estado inicial ni el ID.
            };
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditoria.Agregar("MARCA_CREADA", "marcas", marca.IdMarca, null,
                new Dictionary<string, object?> { ["nombre"] = marca.Nombre, ["idCategoria"] = marca.IdCategoria, ["urlImagen"] = marca.UrlImagen, ["estado"] = marca.Estado }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            commitIniciado = true;
            await transaction.CommitAsync(cancellationToken);

            return new RespuestaMarcaDto
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
            // Conservar el archivo si el resultado de la confirmación es incierto.
            if (archivoGuardado != null && !commitIniciado) _imagenes.Eliminar(archivoGuardado);
            throw;
        }
    }
}
