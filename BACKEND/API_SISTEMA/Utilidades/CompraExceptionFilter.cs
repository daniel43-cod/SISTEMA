using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Utilidades;

public sealed class CompraExceptionFilter(ILogger<CompraExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is CompraOperacionConflictException conflicto)
        {
            context.Result = new ConflictObjectResult(new { mensaje = conflicto.Message });
        }
        else if (context.Exception is CompraValidationException validation)
        {
            context.Result = new BadRequestObjectResult(new { mensaje = validation.Message });
        }
        // Caja cerrada es un rechazo de negocio, no un error interno del servidor.
        else if (context.Exception is CajaValidationException caja)
        {
            context.Result = new BadRequestObjectResult(new { mensaje = caja.Message });
        }
        else if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            context.Result = new StatusCodeResult(499);
        }
        else if (context.Exception is DbUpdateConcurrencyException ||
            (context.Exception.GetBaseException() is PostgresException sql &&
             sql.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected or PostgresErrorCodes.LockNotAvailable or PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation))
        {
            logger.LogWarning(context.Exception, "Conflicto en compras. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
            context.Result = new ConflictObjectResult(new
            {
                mensaje = "No se pudo confirmar la operación por un conflicto de datos. Actualiza y vuelve a intentarlo.",
                traceId = context.HttpContext.TraceIdentifier
            });
        }
        else
        {
            var traceId = context.HttpContext.TraceIdentifier;
            logger.LogError(context.Exception, "Error en compras. Referencia: {TraceId}", traceId);
            context.Result = new ObjectResult(new
            {
                mensaje = "No se pudo completar la operación de compra. Inténtalo de nuevo.",
                traceId
            }) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        context.ExceptionHandled = true;
    }
}
