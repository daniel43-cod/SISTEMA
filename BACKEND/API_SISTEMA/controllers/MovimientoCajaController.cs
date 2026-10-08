using System.ComponentModel.DataAnnotations;
using API_SISTEMA.DTOs.MovimientoCaja;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Administrador)] // El resumen financiero es exclusivamente administrativo.
[TypeFilter(typeof(CajaExceptionFilter))] // No expone mensajes SQL ni trazas al cliente.
[EnableRateLimiting("caja")]
public class MovimientoCajaController(ListarMovimientoCajaService service) : ControllerBase
{
    [HttpGet("listar")]
    [ProducesResponseType(typeof(ResumenMovimientosCajaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarMovimientos(
        [FromQuery, Range(1, int.MaxValue)] int idSesionCaja, CancellationToken cancellationToken)
    {
        // La sesion es explicita: permite revisar el turno actual o uno cerrado sin cambiar de turno accidentalmente.
        var resumen = await service.ConsultarResumen(idSesionCaja, cancellationToken);
        return resumen is null
            ? NotFound(new { mensaje = "La sesion de caja no existe." })
            : Ok(resumen);
    }
}
