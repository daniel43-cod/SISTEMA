using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Marcas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Marca;

public sealed class EstadoMarcaService(SistemaDbContext context)
{
    public async Task<RespuestaMarcaDTO?> CambiarEstado(int id, CambiarEstadoMarcaDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("El ID debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("El estado es obligatorio.");
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var marca = await context.Marcas.Include(m => m.Categoria)
            .SingleOrDefaultAsync(m => m.IdMarca == id, cancellationToken);
        if (marca is null) return null;
        marca.Estado = dto.Estado!.Value;
        await context.SaveChangesAsync(cancellationToken);
        return new RespuestaMarcaDTO
        {
            IdMarca = marca.IdMarca, Nombre = marca.Nombre, IdCategoria = marca.IdCategoria,
            NombreCategoria = marca.Categoria.nombreCategoria,
            Estado = marca.Estado, UrlImagen = marca.UrlImagen
        };
    }
}
