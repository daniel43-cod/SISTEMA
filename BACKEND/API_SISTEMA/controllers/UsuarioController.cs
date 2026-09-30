using API_SISTEMA.DTOs.Login;
using API_SISTEMA.services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Administrador)]
[TypeFilter(typeof(UsuarioExceptionFilter))]
public class UsuarioController(UsuarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListarUsuario() => Ok(await service.ListarUsuario());

    [HttpPost]
    public async Task<IActionResult> Crear(CrearCuentaDTOs dto) => Ok(await service.CrearUsuario(dto));
}