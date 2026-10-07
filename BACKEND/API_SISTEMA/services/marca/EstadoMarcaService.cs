using System.Data;
using API_SISTEMA.services.Auditoria;
using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Marcas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Marca;

public sealed class EstadoMarcaService(SistemaDbContext context, CatalogoAuditoriaService auditoria)
{
    public async Task<RespuestaMarcaDTO?> CambiarEstado(int id, CambiarEstadoMarcaDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("El ID debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("El estado es obligatorio.");
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var marca = await context.Marcas.Include(m => m.Categoria)
            .SingleOrDefaultAsync(m => m.IdMarca == id, cancellationToken);
        if (marca is null) return null;
        var anteriores = new Dictionary<string, object?> { ["estado"] = marca.Estado };
        marca.Estado = dto.Estado!.Value;
        await auditoria.Agregar(marca.Estado ? "MARCA_ACTIVADA" : "MARCA_DESACTIVADA",
            "marcas", marca.IdMarca, anteriores, new Dictionary<string, object?> { ["estado"] = marca.Estado }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new RespuestaMarcaDTO
        {
            IdMarca = marca.IdMarca, Nombre = marca.Nombre, IdCategoria = marca.IdCategoria,
            NombreCategoria = marca.Categoria.nombreCategoria,
            Estado = marca.Estado, UrlImagen = marca.UrlImagen
        };
    }
}
