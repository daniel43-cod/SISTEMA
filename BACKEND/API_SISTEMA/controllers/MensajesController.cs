using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API_SISTEMA.DTOs.Mensaje;
using API_SISTEMA.services.Conversacion;
using API_SISTEMA.services.IA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CLIENTE")]
    public class MensajesController : ControllerBase
    {
        private readonly ConversacionService _service;
        private readonly ILogger<MensajesController> _logger;

        public MensajesController(ConversacionService service, ILogger<MensajesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RespuestaMensajeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<RespuestaMensajeDto>> EnviarMensaje([FromBody] EnviarMensajeDto dto, CancellationToken cancellationToken)
        {
            if (User.FindFirstValue("tipo_cuenta") != "cuenta_cliente")
                return Forbid();

            var identificador = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(identificador, out var idCuentaCliente) || idCuentaCliente <= 0)
                return Unauthorized();

            try
            {
                var respuesta = await _service.EnviarMensaje(dto, idCuentaCliente, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, respuesta);
            }
            catch (OpenAIException ex)
            {
                _logger.LogWarning("Fallo de IA {StatusCode} para mensaje {IdMensaje}", ex.StatusCode, ex.IdMensaje);
                return Problem(statusCode: ex.StatusCode, title: "El asistente no pudo responder",
                    detail: ex.Message, extensions: new Dictionary<string, object?>
                    {
                        ["idMensaje"] = ex.IdMensaje,
                        ["idConversacion"] = ex.IdConversacion
                    });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                // Mismo resultado para una conversación inexistente o de otra cuenta.
                return Problem(statusCode: StatusCodes.Status404NotFound,
                    title: "Conversación no disponible",
                    detail: "La conversación no está disponible para esta cuenta.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar un mensaje.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError,
                    title: "No se pudo guardar el mensaje",
                    detail: "Ocurrió un error interno. Intenta nuevamente más tarde.");
            }
        }
    }
}
