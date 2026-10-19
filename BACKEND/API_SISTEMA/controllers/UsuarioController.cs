using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Administrador)]
[TypeFilter(typeof(UsuarioExceptionFilter))]
public class UsuarioController(UsuarioService service) : ControllerBase
{
    [HttpPatch("{id:int}/estado")]
    [EnableRateLimiting("estado-usuarios")]
    [ProducesResponseType(typeof(UsuarioRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, UsuarioEstadoDto dto,
        [FromServices] API_SISTEMA.Services.Usuarios.UsuarioEstadoService estados, CancellationToken ct)
    {
        var result = await estados.CambiarEstado(id, dto, ct);
        return result is null ? NotFound(new { mensaje = "El usuario no existe." }) : Ok(result);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(List<UsuarioRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuario(CancellationToken cancellationToken) =>
        Ok(await service.ListarUsuario(cancellationToken));

    [HttpPost]
    [EnableRateLimiting("crear-usuarios")]
    public async Task<IActionResult> Crear(CrearCuentaDto dto, CancellationToken cancellationToken) => Ok(await service.CrearUsuario(dto, cancellationToken));

    [HttpPut("{id:int}")]
    [EnableRateLimiting("crear-usuarios")]
    [ProducesResponseType(typeof(UsuarioRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Editar(int id, EditarUsuarioDto dto,
        [FromServices] API_SISTEMA.Services.Usuarios.UsuarioEditarService editar,
        CancellationToken cancellationToken)
    {
        var result = await editar.EditarUsuario(id, dto, cancellationToken);
        return result is null ? NotFound(new { mensaje = "El usuario no existe." }) : Ok(result);
    }
}
