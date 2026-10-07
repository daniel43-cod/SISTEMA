using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Presentaciones;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Prestacion;

public sealed class EstadoPresentacionService(SistemaDbContext context, API_SISTEMA.services.Auditoria.PresentacionAuditoriaService auditoria)
{
    public async Task<PresentacionRespuestaDTO?> CambiarEstado(int id, CambiarEstadoPresentacionDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("El ID debe ser mayor que cero.");
        if (dto is null) throw new ValidationException("El estado es obligatorio.");
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var presentacion = await context.presentaciones.SingleOrDefaultAsync(p => p.IdPresentacion == id, cancellationToken);
        if (presentacion is null) return null;
        if (presentacion.Estado != dto.Estado!.Value)
        {
            var anterior = new { estado = presentacion.Estado };
            presentacion.Estado = dto.Estado.Value;
            await auditoria.Agregar(presentacion.Estado.Value ? "PRESENTACION_ACTIVADA" : "PRESENTACION_DESACTIVADA",
                presentacion, anterior, new { estado = presentacion.Estado }, cancellationToken);
        }
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new PresentacionRespuestaDTO
        {
            IdPresentacion = presentacion.IdPresentacion,
            Descripcion = presentacion.Descripcion, Estado = presentacion.Estado.Value
        };
    }
}
