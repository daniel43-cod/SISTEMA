using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Utilidades;

public sealed class VentaConflictoException(string mensaje) : Exception(mensaje);
public sealed class VentaExceptionFilter(ILogger<VentaExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var status = ex switch
        {
            ValidationException => 400, CajaValidationException => 409, VentaConflictoException => 409,
            UnauthorizedAccessException => 403, KeyNotFoundException => 404,
            OperationCanceledException when context.HttpContext.RequestAborted.IsCancellationRequested => 499,
            DbUpdateConcurrencyException => 409,
            _ when ex.GetBaseException() is SqlException sql && sql.Number is 1205 or 1222 or 2601 or 2627 => 409,
            _ => 500
        };
        if (status == 500) logger.LogError(ex, "Error en ventas. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
        else if (status == 409) logger.LogWarning(ex, "Conflicto en ventas. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
        var mensaje = status is 400 or 403 or 404 || ex is CajaValidationException or VentaConflictoException ? ex.Message
            : status == 409 ? "Conflicto de datos. Consulta el resultado antes de reintentar con la misma clave de operación."
            : "No se pudo confirmar la operación. Consulta el resultado antes de reenviarla.";
        context.Result = new ObjectResult(new { mensaje, traceId = context.HttpContext.TraceIdentifier }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
