using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Utilidades;

public sealed class CajaExceptionFilter(ILogger<CajaExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is CajaValidationException validation)
        {
            context.Result = new BadRequestObjectResult(new { mensaje = validation.Message });
        }
        else if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            context.Result = new StatusCodeResult(499);
        }
        else if (context.Exception is DbUpdateConcurrencyException ||
            (context.Exception.GetBaseException() is PostgresException sql &&
             sql.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected or PostgresErrorCodes.LockNotAvailable or PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation))
        {
            logger.LogWarning(context.Exception, "Conflicto en caja. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
            context.Result = new ConflictObjectResult(new
            {
                mensaje = "No se pudo confirmar la operación por un conflicto de datos. Actualiza y vuelve a intentarlo.",
                traceId = context.HttpContext.TraceIdentifier
            });
        }
        else
        {
            var traceId = context.HttpContext.TraceIdentifier;
            logger.LogError(context.Exception, "Error en caja. Referencia: {TraceId}", traceId);
            context.Result = new ObjectResult(new
            {
                mensaje = "No se pudo completar la operación de caja. Inténtalo de nuevo.",
                traceId
            }) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        context.ExceptionHandled = true;
    }
}
