using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API_SISTEMA.Dtos.Conversacion;
using API_SISTEMA.Services.Conversacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "CLIENTE")]
public sealed class ConversacionesController(ConversacionListarService service,
    ILogger<ConversacionesController> logger) : ControllerBase
{
    [HttpDelete("{idConversacion:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EliminarChat([Range(1, int.MaxValue)] int idConversacion,
        [FromServices] ConversacionEliminarService eliminarService, CancellationToken cancellationToken)
    {
        if (User.FindFirstValue("tipo_cuenta") != "cuenta_cliente") return Forbid();
        var identificador = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(identificador, out var cuenta) || cuenta <= 0) return Unauthorized();
        try
        {
            return await eliminarService.Eliminar(cuenta, idConversacion, cancellationToken)
                ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error al eliminar conversación {IdConversacion}", idConversacion);
            return Problem(statusCode: 500, title: "No se pudo eliminar la conversación.");
        }
    }

    [HttpGet("{idConversacion:int}/mensajes")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(List<MensajeChatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<MensajeChatDto>>> ListarMensajes(
        [Range(1, int.MaxValue)] int idConversacion,
        [FromQuery, Range(1, int.MaxValue)] int? antesDe = null,
        [FromQuery, Range(1, 100)] int limite = 50,
        CancellationToken cancellationToken = default)
    {
        if (User.FindFirstValue("tipo_cuenta") != "cuenta_cliente") return Forbid();
        var identificador = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(identificador, out var idCuentaCliente) || idCuentaCliente <= 0)
            return Unauthorized();

        try
        {
            var mensajes = await service.ListarMensajes(
                idCuentaCliente, idConversacion, antesDe, limite, cancellationToken);
            // No distinguir una conversación ajena de una inexistente.
            if (mensajes is null) return NotFound();
            return Ok(mensajes);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error al listar mensajes de conversación {IdConversacion}", idConversacion);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "No se pudieron cargar los mensajes.");
        }
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(List<ChatClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ChatClienteDto>>> ListarChats(
        [FromQuery, Range(1, 100000)] int pagina = 1,
        [FromQuery, Range(1, 100)] int tamanoPagina = 25,
        CancellationToken cancellationToken = default)
    {
        if (User.FindFirstValue("tipo_cuenta") != "cuenta_cliente") return Forbid();
        var identificador = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(identificador, out var idCuentaCliente) || idCuentaCliente <= 0)
            return Unauthorized();
        try
        {
            return Ok(await service.ListarChats(idCuentaCliente, pagina, tamanoPagina, cancellationToken));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error al listar chats de cuenta {IdCuentaCliente}", idCuentaCliente);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "No se pudieron cargar las conversaciones.");
        }
    }
}
