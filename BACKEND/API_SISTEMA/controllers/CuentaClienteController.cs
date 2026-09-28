using System.ComponentModel.DataAnnotations;
using API_SISTEMA.DTOs.RegistrCliente;
using API_SISTEMA.services.CuentaCliente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentaClienteController : ControllerBase
    {
        private readonly RegistroCuentaClienteService _service;
        private readonly InicioSesionService _inicioSesionService;
        private readonly ILogger<CuentaClienteController> _logger;

        public CuentaClienteController(RegistroCuentaClienteService service, ILogger<CuentaClienteController> logger,
            InicioSesionService inicioSesionService)
        {
            _service = service;
            _logger = logger;
            _inicioSesionService = inicioSesionService;
        }

        [AllowAnonymous]
        [HttpPost("iniciosesion")]
        [ProducesResponseType(typeof(InicioSesionClienteRespuestaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InicioSesionClienteRespuestaDto>> IniciarSesion(
            [FromBody] InicioSesionClienteDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var respuesta = await _inicioSesionService.IniciarSesion(dto, cancellationToken);
                if (respuesta == null)
                    return Problem(statusCode: StatusCodes.Status401Unauthorized,
                        title: "No se pudo iniciar sesión",
                        detail: "Correo o contraseña incorrectos, o cuenta no disponible.");

                return Ok(respuesta);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión de una cuenta de cliente.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError,
                    title: "No se pudo iniciar sesión",
                    detail: "Ocurrió un error interno. Intenta nuevamente más tarde.");
            }
        }

        [AllowAnonymous]
        [HttpPost("registro")]
        [ProducesResponseType(typeof(CuentaClienteCreadaDto), StatusCodes.Status201Created)]//cuenta creada
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]//datos invalidos
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]//correo ya registrado
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]//error interno
        public async Task<ActionResult<CuentaClienteCreadaDto>> Registrarse([FromBody] RegistroCuentaClienteDto dto,CancellationToken cancellationToken)
        {
            try
            {
                var cuenta = await _service.Registro(dto, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, cuenta);
                
                
                
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
            catch (CorreoClienteEnUsoException ex)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Correo ya registrado",
                    detail: ex.Message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar una cuenta de cliente.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "No se pudo crear la cuenta",
                    detail: "Ocurrió un error interno. Intenta nuevamente más tarde.");
            }
        }
    }
}
