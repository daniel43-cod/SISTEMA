using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Administrador)]
[TypeFilter(typeof(UsuarioExceptionFilter))]
public class UsuarioController(UsuarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListarUsuario() => Ok(await service.ListarUsuario());

    [HttpPost]
    public async Task<IActionResult> Crear(CrearCuentaDto dto) => Ok(await service.CrearUsuario(dto));
}