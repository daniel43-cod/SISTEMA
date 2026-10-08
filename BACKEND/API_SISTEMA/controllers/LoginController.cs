using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(UsuarioExceptionFilter))]
public class LoginController(LoginService loginService, UsuarioService usuarioService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login-interno")]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var respuesta = await loginService.Login(dto, cancellationToken);
        return respuesta is null
            ? Unauthorized("Usuario o contraseña incorrectos.")
            : Ok(respuesta);
    }

    // Alias de compatibilidad: ambas rutas ejecutan las mismas validaciones.
    [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
    [HttpPost("cerrar-sesion")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> CerrarSesion(
        [FromServices] API_SISTEMA.Services.Auditoria.AuditoriaService auditoria,
        CancellationToken cancellationToken)
        => await auditoria.RegistrarCierreSesion(cancellationToken) ? NoContent() : Unauthorized();

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost("crear")]
    public async Task<IActionResult> Crear(CrearCuentaDto dto)
    {
        await usuarioService.CrearUsuario(dto);
        return Ok(new { mensaje = "Usuario creado correctamente." });
    }
}
