using System.ComponentModel.DataAnnotations;
using API_SISTEMA.DTOs.Marcas;
using API_SISTEMA.services.Marca;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcasController : ControllerBase
{
    private readonly MarcaCrearService _crearService;
    private readonly MarcaListarService _listarService;
    private readonly ILogger<MarcasController> _logger;

    public MarcasController(MarcaCrearService crearService, ILogger<MarcasController> logger,
        MarcaListarService listarService)
    {
        _crearService = crearService;
        _listarService = listarService;
        _logger = logger;
    }

    // Administradores y vendedores pueden consultar las marcas activas.
    [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
    [HttpGet]
    public async Task<ActionResult<List<RespuestaMarcaDTO>>> Listar(CancellationToken cancellationToken)
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

    // La creación sigue siendo exclusiva de administradores.
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<ActionResult<RespuestaMarcaDTO>> Crear([FromBody] CrearMarcaDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var marca = await _crearService.CrearMarca(dto, cancellationToken);
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
