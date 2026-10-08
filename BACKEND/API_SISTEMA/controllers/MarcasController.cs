using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Dtos.Marcas;
using API_SISTEMA.Services.Marca;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcasController : ControllerBase
{
    private readonly MarcaActualizarService _actualizarService;
    private readonly MarcaCrearService _crearService;
    private readonly MarcaListarService _listarService;
    private readonly ILogger<MarcasController> _logger;

    public MarcasController(MarcaCrearService crearService, ILogger<MarcasController> logger,
        MarcaListarService listarService, MarcaActualizarService actualizarService)
    {
        _crearService = crearService;
        _actualizarService = actualizarService;
        _listarService = listarService;
        _logger = logger;
    }

    // Administradores y vendedores pueden consultar las marcas activas.
    [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
    [HttpGet]
    public async Task<ActionResult<List<RespuestaMarcaDto>>> Listar(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _listarService.ListarActivas(cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            _logger.LogError(ex, "Error al listar marcas. Referencia: {TraceId}", traceId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                mensaje = "No se pudieron listar las marcas.", traceId
            });
        }
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPatch("{idMarca:int}/estado")]
    [Consumes("application/json")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> CambiarEstado([Range(1, int.MaxValue)] int idMarca,
        [FromBody] CambiarEstadoMarcaDto dto, [FromServices] EstadoMarcaService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await service.CambiarEstado(idMarca, dto, cancellationToken);
            if (resultado is null) return NotFound(new { mensaje = "La marca no existe." });
            return Ok(resultado);
        }
        catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            _logger.LogError(ex, "Error al cambiar estado de marca. Referencia: {TraceId}", traceId);
            return StatusCode(500, new { mensaje = "No se pudo cambiar el estado de la marca.", traceId });
        }
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpGet("administracion")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ListarAdministracion(CancellationToken cancellationToken)
    {
        try { return Ok(await _listarService.ListarAdministracion(cancellationToken)); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            _logger.LogError(ex, "Error al listar marcas para administración. Referencia: {TraceId}", traceId);
            return StatusCode(500, new { mensaje = "No se pudieron listar las marcas.", traceId });
        }
    }

    // La edición valida permisos en la API, aunque el frontend oculte el botón.
    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{idMarca:int}")]
    [Consumes("application/json")]
    public Task<ActionResult<RespuestaMarcaDto>> Actualizar(int idMarca,
        [FromBody] ActualizarMarcaDto dto, CancellationToken cancellationToken)
        => ActualizarInterno(idMarca, dto, null, cancellationToken);

    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{idMarca:int}/con-imagen")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public Task<ActionResult<RespuestaMarcaDto>> ActualizarConImagen(int idMarca,
        [FromForm] ActualizarMarcaConImagenDto dto, CancellationToken cancellationToken)
        => ActualizarInterno(idMarca, dto, dto.Imagen, cancellationToken);

    private async Task<ActionResult<RespuestaMarcaDto>> ActualizarInterno(int idMarca,
        ActualizarMarcaDto dto, IFormFile? imagen, CancellationToken cancellationToken)
    {
        try
        {
            var marca = await _actualizarService.ActualizarMarca(idMarca, dto, cancellationToken, imagen);
            if (marca is null) return NotFound(new { mensaje = "La marca no existe." });
            return Ok(marca);
        }
        catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (MarcaDuplicadaException ex) { return Conflict(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var traceId = HttpContext.TraceIdentifier;
            _logger.LogError(ex, "Error al actualizar marca. Referencia: {TraceId}", traceId);
            return StatusCode(500, new { mensaje = "No se pudo actualizar la marca.", traceId });
        }
    }
    // La creación sigue siendo exclusiva de administradores.
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [Consumes("application/json")]
    public Task<ActionResult<RespuestaMarcaDto>> Crear([FromBody] CrearMarcaDto dto,
        CancellationToken cancellationToken)
        => CrearInterno(dto, null, cancellationToken);

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost("con-imagen")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public Task<ActionResult<RespuestaMarcaDto>> CrearConImagen([FromForm] CrearMarcaConImagenDto dto,
        CancellationToken cancellationToken)
        => CrearInterno(dto, dto.Imagen, cancellationToken);

    private async Task<ActionResult<RespuestaMarcaDto>> CrearInterno(CrearMarcaDto dto,
        IFormFile? imagen, CancellationToken cancellationToken)
    {
        try
        {
            var marca = await _crearService.CrearMarca(dto, cancellationToken, imagen);
            return StatusCode(StatusCodes.Status201Created, marca);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (MarcaDuplicadaException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Registrar el detalle técnico sin exponerlo al cliente.
            var traceId = HttpContext.TraceIdentifier;
            _logger.LogError(ex, "Error al crear marca. Referencia: {TraceId}", traceId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                mensaje = "No se pudo crear la marca.", traceId
            });
        }
    }
}

