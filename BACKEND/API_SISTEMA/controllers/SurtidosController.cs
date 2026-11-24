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
        [FromQuery] string? texto = null, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, [FromQuery] int? idMarca = null)
        => Ok(await service.Listar(texto, pagina, tamano, ct, idMarca));

    [HttpPost("con-imagen")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<SurtidoDto>> CrearConImagen([FromForm] CrearSurtidoConImagenDto dto, CancellationToken ct)
    {
        var resultado = await service.Crear(dto, ct, dto.Imagen);
        return CreatedAtAction(nameof(Consultar), new { id = resultado.IdSurtido }, resultado);
    }

    [HttpPost]
    public async Task<ActionResult<SurtidoDto>> Crear(CrearSurtidoDto dto, CancellationToken ct)
    {
        var resultado = await service.Crear(dto, ct);
        return CreatedAtAction(nameof(Consultar), new { id = resultado.IdSurtido }, resultado);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<SurtidoDto>> Consultar(int id, CancellationToken ct)
        => Ok(await service.Consultar(id, ct));

    [HttpPut("{id:int:min(1)}")]
    public async Task<ActionResult<SurtidoDto>> Actualizar(int id, ActualizarSurtidoDto dto, CancellationToken ct)
        => Ok(await service.Actualizar(id, dto, ct));

    [HttpPut("{id:int:min(1)}/con-imagen")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<SurtidoDto>> ActualizarConImagen(int id, [FromForm] ActualizarSurtidoConImagenDto dto, CancellationToken ct)
        => Ok(await service.Actualizar(id, dto, ct, dto.Imagen));
}
