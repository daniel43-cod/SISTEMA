using System.ComponentModel.DataAnnotations;
using API_SISTEMA.DTOs.Marcas;
using API_SISTEMA.services.Marca;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Administrador)]
public class MarcasController : ControllerBase
{
    private readonly MarcaCrearService _crearService;
    private readonly ILogger<MarcasController> _logger;

    public MarcasController(MarcaCrearService crearService, ILogger<MarcasController> logger)
    {
        _crearService = crearService;
        _logger = logger;
    }

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
