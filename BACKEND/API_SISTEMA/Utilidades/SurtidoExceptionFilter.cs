using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API_SISTEMA.Utilidades;

public sealed class SurtidoExceptionFilter(ILogger<SurtidoExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var status = ex switch
        {
            ValidationException => 400,
            KeyNotFoundException => 404,
            OperationCanceledException when context.HttpContext.RequestAborted.IsCancellationRequested => 499,
            DbUpdateConcurrencyException => 409,
            _ when ex.GetBaseException() is PostgresException pg && pg.SqlState is
                PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected => 409,
            _ => 500
        };
        if (status == 500) logger.LogError(ex, "Error en surtidos. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
        context.Result = new ObjectResult(new
        {
            mensaje = status is 400 or 404 ? ex.Message : status == 409
                ? "Los datos cambiaron durante la operación. Consulta el surtido antes de reintentar."
                : "No se pudo completar la operación de surtidos.",
            traceId = context.HttpContext.TraceIdentifier
        }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
