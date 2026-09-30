using System.ComponentModel.DataAnnotations;
using API_SISTEMA.DTOs.Presentaciones;
using API_SISTEMA.services.Prestacion;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
public class PresentacionesController(CrearPresentacionServices crearService,ListarPresentacionServices listarService,ILogger<PresentacionesController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Crear(CrearPresentacionDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var presentacion = await crearService.CrearPresentacion(dto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, presentacion);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (PresentacionDuplicadaException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ErrorInterno(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await listarService.ListarActivas(cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ErrorInterno(ex);
        }
    }

    private ObjectResult ErrorInterno(Exception ex)
    {
        var traceId = HttpContext.TraceIdentifier;
        logger.LogError(ex, "Error al procesar presentaciones. Referencia: {TraceId}", traceId);
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            mensaje = "No se pudo completar la operación de presentaciones. Inténtalo de nuevo.",
            traceId
        });
    }
}
