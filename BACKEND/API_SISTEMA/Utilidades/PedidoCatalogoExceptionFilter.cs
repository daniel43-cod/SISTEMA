using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Services.PedidosCatalogo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Utilidades;

public sealed class PedidoCatalogoExceptionFilter(ILogger<PedidoCatalogoExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var status = ex switch
        {
            ValidationException => 400,
            PedidoCatalogoConflictoException => 409,
            DbUpdateConcurrencyException => 409,
            OperationCanceledException when context.HttpContext.RequestAborted.IsCancellationRequested => 499,
            _ when ex.GetBaseException() is SqlException sql && sql.Number is 1205 or 1222 or 2601 or 2627 => 409,
            _ => 500
        };
        if (status == 500) logger.LogError(ex, "Error en pedidos del catálogo. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
        var mensaje = ex is ValidationException or PedidoCatalogoConflictoException ? ex.Message
            : HttpMethods.IsGet(context.HttpContext.Request.Method)
                ? "No se pudieron consultar los pedidos del catálogo. Intenta nuevamente más tarde."
                : "No se pudo confirmar el pedido. Conserva la misma clave de operación al reintentar.";
        context.Result = new ObjectResult(new { mensaje, traceId = context.HttpContext.TraceIdentifier }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
