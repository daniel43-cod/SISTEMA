using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Presentaciones;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Prestacion;

public class ActualizarPresentacionService(SistemaDbContext context)
{
    // Devuelve null cuando el ID no existe, para que el controlador responda 404.
    public async Task<PresentacionRespuestaDTO?> ActualizarPresentacion(int idPresentacion,ActualizarPresentacionDTO dto,
        CancellationToken cancellationToken = default)
    {
        if (idPresentacion <= 0)
            throw new ValidationException("El ID de la presentación debe ser mayor que cero.");

        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

        var descripcion = dto.Descripcion.Trim();
        var descripcionNormalizada = descripcion.ToUpperInvariant();

        // Mantiene la comprobación de duplicados y la actualización en la misma transacción.
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var presentacion = await context.presentaciones
            .SingleOrDefaultAsync(p => p.IdPresentacion == idPresentacion, cancellationToken);

        if (presentacion is null)
            return null;

        var existe = await context.presentaciones.AnyAsync(p =>
            p.IdPresentacion != idPresentacion &&
            p.Descripcion != null &&
            p.Descripcion.Trim().ToUpper() == descripcionNormalizada, cancellationToken);

        if (existe)
            throw new PresentacionDuplicadaException();

        presentacion.Descripcion = descripcion;

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PresentacionRespuestaDTO
        {
            IdPresentacion = presentacion.IdPresentacion,
            Descripcion = presentacion.Descripcion,
            Estado = presentacion.Estado == true
        };
    }
}
