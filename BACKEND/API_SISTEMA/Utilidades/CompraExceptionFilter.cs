using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API_SISTEMA.Utilidades;

public sealed class CompraExceptionFilter(ILogger<CompraExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is CompraValidationException validation)
        {
            context.Result = new BadRequestObjectResult(new { mensaje = validation.Message });
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
