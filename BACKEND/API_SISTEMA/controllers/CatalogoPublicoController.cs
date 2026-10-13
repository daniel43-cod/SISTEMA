using API_SISTEMA.Dtos.CatalogoPublico;
using API_SISTEMA.Services.CatalogoPublico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogoPublicoController(CatalogoPublicoService service,ILogger<CatalogoPublicoController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("marcas/{idMarca:int}/productos")]
    [ProducesResponseType(typeof(ProductosPublicosPaginaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductosPublicosPaginaDto>> ListarProductosPorMarca(
        [Range(1, int.MaxValue)] int idMarca,
        [FromQuery, Range(1, 100000)] int pagina = 1,
        [FromQuery, Range(1, 60)] int tamanoPagina = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resultado = await service.ListarProductosPorMarca(idMarca, pagina, tamanoPagina, cancellationToken);
            if (resultado is null)
                return Problem(statusCode: StatusCodes.Status404NotFound, title: "La marca no está disponible.");
            return Ok(resultado);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            logger.LogError(ex, "Error al listar productos públicos de marca {IdMarca}. Referencia: {TraceId}",
                idMarca, traceId);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "No se pudieron listar los productos.",
                extensions: new Dictionary<string, object?> { ["traceId"] = traceId });
        }
    }

    [AllowAnonymous]
    [HttpGet("categorias/{idCategoria:int}/marcas")]
    [ProducesResponseType(typeof(List<MarcaPublicaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<MarcaPublicaDto>>> ListarMarcasPorCategoria(
        [Range(1, int.MaxValue)] int idCategoria, CancellationToken cancellationToken)
    {
        try
        {
            var marcas = await service.ListarMarcasPorCategoria(idCategoria, cancellationToken);
            if (marcas is null)
                return Problem(statusCode: StatusCodes.Status404NotFound,
                    title: "La categoría no está disponible.");

            return Ok(marcas);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            logger.LogError(ex, "Error al listar marcas públicas de categoría {IdCategoria}. Referencia: {TraceId}",
                idCategoria, traceId);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "No se pudieron listar las marcas.",
                extensions: new Dictionary<string, object?> { ["traceId"] = traceId });
        }
    }

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
