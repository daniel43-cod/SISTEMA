using API_SISTEMA.DTOs.Surtidos;
using API_SISTEMA.Services.Surtidos;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/surtidos")]
[Authorize(Roles = Roles.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[TypeFilter(typeof(SurtidoExceptionFilter))]
public sealed class SurtidosController(SurtidoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ListaSurtidosDto>> Listar(CancellationToken ct,
        [FromQuery] string? texto = null, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        => Ok(await service.Listar(texto, pagina, tamano, ct));

    [HttpPost]
    public async Task<ActionResult<SurtidoDto>> Crear(CrearSurtidoDto dto, CancellationToken ct)
    {
        var resultado = await service.Crear(dto, ct);
        return CreatedAtAction(nameof(Consultar), new { id = resultado.IdSurtido }, resultado);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<SurtidoDto>> Consultar(int id, CancellationToken ct)
        => Ok(await service.Consultar(id, ct));
}
