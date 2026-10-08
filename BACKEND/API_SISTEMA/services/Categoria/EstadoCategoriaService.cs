using System.Data;
using API_SISTEMA.Services.Auditoria;
using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Categoria;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Categoria;

public sealed class EstadoCategoriaService(SistemaDbContext context, CatalogoAuditoriaService auditoria)
{
    public async Task<RespuestaCategoriaDto?> CambiarEstado(int id, CambiarEstadoCategoriaDto dto,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("El ID debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("El estado es obligatorio.");
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var categoria = await context.categorias.SingleOrDefaultAsync(c => c.IdCategoria == id, cancellationToken);
        if (categoria is null) return null;
        var anteriores = new Dictionary<string, object?> { ["estado"] = categoria.Estado };
        categoria.Estado = dto.Estado!.Value;
        await auditoria.Agregar(categoria.Estado ? "CATEGORIA_ACTIVADA" : "CATEGORIA_DESACTIVADA",
            "categorias", categoria.IdCategoria, anteriores, new Dictionary<string, object?> { ["estado"] = categoria.Estado }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new RespuestaCategoriaDto
        {
            IdCategoria = categoria.IdCategoria, Nombre = categoria.nombreCategoria,
            Estado = categoria.Estado, UrlImagen = categoria.UrlImagen
        };
    }
}
