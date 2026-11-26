using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API_SISTEMA.Utilidades;

public sealed class ClienteExceptionFilter(ILogger<ClienteExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested)
            context.Result = new StatusCodeResult(499);
        else if (context.Exception is System.ComponentModel.DataAnnotations.ValidationException or UnauthorizedAccessException or KeyNotFoundException)
        {
            var status = context.Exception is UnauthorizedAccessException ? 403
                : context.Exception is KeyNotFoundException ? 404 : 400;
            context.Result = new ObjectResult(new { mensaje = context.Exception.Message }) { StatusCode = status };
        }
        else
        {
            logger.LogError(context.Exception, "Error consultando clientes. Referencia: {TraceId}", context.HttpContext.TraceIdentifier);
            context.Result = new ObjectResult(new { mensaje = "No se pudo consultar la información del cliente.", traceId = context.HttpContext.TraceIdentifier })
            { StatusCode = 500 };
        }
        context.ExceptionHandled = true;
    }
}
