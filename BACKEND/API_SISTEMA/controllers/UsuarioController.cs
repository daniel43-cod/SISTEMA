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
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(List<UsuarioRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuario(CancellationToken cancellationToken) =>
        Ok(await service.ListarUsuario(cancellationToken));

    [HttpPost]
    [EnableRateLimiting("crear-usuarios")]
    public async Task<IActionResult> Crear(CrearCuentaDto dto, CancellationToken cancellationToken) => Ok(await service.CrearUsuario(dto, cancellationToken));
}
