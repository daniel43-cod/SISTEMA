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

    public MarcaActualizarService(SistemaDbContext context)
    {
        _context = context;
    }

    public async Task<RespuestaMarcaDTO?> ActualizarMarca(int idMarca, ActualizarMarcaDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (idMarca <= 0) throw new ValidationException("El ID de la marca debe ser mayor que cero.");
        if (dto is null)
            throw new ValidationException("La información de la marca es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var nombre = dto.Nombre.Trim();
        var normalizado = nombre.ToUpperInvariant();

        // Mantiene las validaciones y la creación dentro de la misma transacción.
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var marca = await _context.Marcas.SingleOrDefaultAsync(m => m.IdMarca == idMarca, cancellationToken);
        if (marca is null) return null;

        var categoriaActiva = await _context.categorias.AnyAsync(
            c => c.IdCategoria == dto.IdCategoria && c.Estado, cancellationToken);
        if (!categoriaActiva)
            throw new ValidationException("La categoría no existe o está inactiva.");

        // La marca es única por nombre, incluso en otra categoría o estando inactiva.
        var duplicada = await _context.Marcas.AnyAsync(
            m => m.IdMarca != idMarca && m.Nombre.Trim().ToUpper() == normalizado, cancellationToken);
        if (duplicada) throw new MarcaDuplicadaException();

        // Actualiza solo nombre y categoría; conserva el estado.
        marca.Nombre = nombre;
        marca.IdCategoria = dto.IdCategoria;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RespuestaMarcaDTO
        {
            IdMarca = marca.IdMarca,
            Nombre = marca.Nombre,
            IdCategoria = marca.IdCategoria,
            Estado = marca.Estado
        };
    }
}

