using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Services.Gastos;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;


namespace API_SISTEMA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GastosController : ControllerBase
    {
        private readonly CrearGastosService _service;
        private readonly ILogger<GastosController> _logger;
        public GastosController(CrearGastosService service, ILogger<GastosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpGet("listar")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(GastoPaginaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GastoPaginaDto>> ListarGastos(
            [FromServices] GastoListarService service,
            [FromQuery, Range(1, 100000)] int pagina = 1,
            [FromQuery, Range(1, 100)] int tamanoPagina = 25,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await service.Listar(pagina, tamanoPagina, cancellationToken));
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (CajaValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al listar gastos. Referencia: {TraceId}", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "No se pudieron consultar los gastos. Inténtalo más tarde.",
                    traceId
                });
            }
        }

        [Authorize(Roles =Roles.Administrador)]
        [HttpPost("crear")]
        [HttpPost("/CrearGasto")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearGasto([FromBody] IngresarGastoDto gastoDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var idUsuarioClaim =User.FindFirstValue(ClaimTypes.NameIdentifier)?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario) || idUsuario <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var idGasto = await _service.CrearGasto(
                    gastoDto,
                    idUsuario,
                    cancellationToken
                );

                return Ok(new
                {
                    mensaje = "Gasto registrado correctamente.",
                    id_gasto = idGasto
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (CajaValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al registrar gasto. Referencia: {TraceId}", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "No se pudo registrar el gasto. Inténtalo más tarde.",
                    traceId
                });
            }
        }
    }
}
