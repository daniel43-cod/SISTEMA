using API_SISTEMA.DTOs.Login;
using API_SISTEMA.services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.controllers;

[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(UsuarioExceptionFilter))]
public class LoginController(LoginService loginService, UsuarioService usuarioService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login-interno")]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDTOs dto)
    {
        var respuesta = await loginService.Login(dto);
        return respuesta is null
            ? Unauthorized("Usuario o contraseña incorrectos.")
            : Ok(respuesta);
    }

    // Alias de compatibilidad: ambas rutas ejecutan las mismas validaciones.
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost("crear")]
    public async Task<IActionResult> Crear(CrearCuentaDTOs dto)
    {
        await usuarioService.CrearUsuario(dto);
        return Ok(new { mensaje = "Usuario creado correctamente." });
    }
}