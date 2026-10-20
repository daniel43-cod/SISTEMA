using API_SISTEMA.Services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
[TypeFilter(typeof(ClienteExceptionFilter))]
public class ClienteController(ClienteService service) : ControllerBase
{
    [HttpGet("listar")]
    public async Task<IActionResult> Listar(CancellationToken ct = default)
        => Ok(await service.ListarClientes(ct));

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct = default)
    {
        var cliente = await service.ObtenerCliente(id, ct);
        return cliente is null ? NotFound(new { mensaje = "El cliente no existe." }) : Ok(cliente);
    }
}
