using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Presentaciones;
using API_SISTEMA.models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Prestacion;

public class CrearPresentacionServices(SistemaDbContext context)
{
    public async Task<PresentacionRespuestaDTO> CrearPresentacion(
        CrearPresentacionDTO dto, CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var descripcion = dto.Descripcion.Trim();
        var descripcionNormalizada = descripcion.ToUpperInvariant();

        // Comprobación y escritura comparten la transacción para evitar altas concurrentes duplicadas.
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var existe = await context.presentaciones.AnyAsync(p => p.Descripcion != null &&
            p.Descripcion.Trim().ToUpper() == descripcionNormalizada, cancellationToken);
        if (existe)
            throw new PresentacionDuplicadaException();

        var presentacion = new Presentacion { Descripcion = descripcion, Estado = true };
        context.presentaciones.Add(presentacion);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PresentacionRespuestaDTO
        {
            IdPresentacion = presentacion.IdPresentacion,
            Descripcion = presentacion.Descripcion,
            Estado = true
        };
    }

}
