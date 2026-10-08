using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Presentaciones;
using API_SISTEMA.Models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Presentaciones;

public class CrearPresentacionService(SistemaDbContext context, API_SISTEMA.Services.Auditoria.PresentacionAuditoriaService auditoria)
{
    public async Task<PresentacionRespuestaDto> CrearPresentacion(
        CrearPresentacionDto dto, CancellationToken cancellationToken = default)
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

        var presentacion = new Presentacion { 
            Descripcion = descripcion, 
            Estado = true };
        context.presentaciones.Add(presentacion);
        await context.SaveChangesAsync(cancellationToken);
        await auditoria.Agregar("PRESENTACION_CREADA", presentacion, null,
            new { descripcion = presentacion.Descripcion, estado = presentacion.Estado }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PresentacionRespuestaDto
        {
            IdPresentacion = presentacion.IdPresentacion,
            Descripcion = presentacion.Descripcion,
            Estado = true
        };
    }

}
