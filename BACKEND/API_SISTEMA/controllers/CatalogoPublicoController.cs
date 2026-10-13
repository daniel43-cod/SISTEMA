using API_SISTEMA.Dtos.CatalogoPublico;
using API_SISTEMA.Services.CatalogoPublico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogoPublicoController(CatalogoPublicoService service,ILogger<CatalogoPublicoController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("categorias")]
    [ProducesResponseType(typeof(List<CategoriaPublicaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CategoriaPublicaDto>>> ListarCategorias(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListarCategorias(cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            logger.LogError(ex, "Error al listar categorías públicas. Referencia: {TraceId}", traceId);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "No se pudieron listar las categorías.",
                extensions: new Dictionary<string, object?> { ["traceId"] = traceId });
        }
    }
}
