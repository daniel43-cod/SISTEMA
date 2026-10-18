using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API_SISTEMA.Utilidades;

public sealed class UsuarioExceptionFilter(ILogger<UsuarioExceptionFilter> logger) : IExceptionFilter
{

    //controlar excepciones
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            context.Result = new StatusCodeResult(499);
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is UsuarioConflictException conflict)
        {
            context.Result = new ConflictObjectResult(new { mensaje = conflict.Message });
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is UsuarioValidationException validation)
            context.Result = new BadRequestObjectResult(new { mensaje = validation.Message });
        else
        {
            var traceId = context.HttpContext.TraceIdentifier;
            logger.LogError(context.Exception, "Error al procesar usuario. Referencia: {TraceId}", traceId);
            context.Result = new ObjectResult(new
            {
                mensaje = "No se pudo completar la operación. Inténtalo de nuevo.", traceId
            }) { StatusCode = StatusCodes.Status500InternalServerError };
        }
        context.ExceptionHandled = true;
    }
}
