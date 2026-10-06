using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Categoria;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Categoria;

public sealed class EstadoCategoriaService(SistemaDbContext context)
{
    public async Task<RespuestaCategoriaDTO?> CambiarEstado(int id, CambiarEstadoCategoriaDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("El ID debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("El estado es obligatorio.");
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var categoria = await context.categorias.SingleOrDefaultAsync(c => c.IdCategoria == id, cancellationToken);
        if (categoria is null) return null;
        categoria.Estado = dto.Estado!.Value;
        await context.SaveChangesAsync(cancellationToken);
        return new RespuestaCategoriaDTO
        {
            IdCategoria = categoria.IdCategoria, Nombre = categoria.nombreCategoria,
            Estado = categoria.Estado, UrlImagen = categoria.UrlImagen
        };
    }
}
