using System.Security.Claims;
using API_SISTEMA.DTOs.Caja;
using API_SISTEMA.services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Administrador)] // Solo administradores administran el turno compartido.
[TypeFilter(typeof(CajaExceptionFilter))] // Nunca se devuelven excepciones SQL ni trazas internas.
[EnableRateLimiting("caja")]
[RequestSizeLimit(16384)]
public class CajaController(CajaService servicio) : ControllerBase
{
    // El selector del frontend recibe solo identificadores y nombres de cajas activas.
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles(CancellationToken ct)
        => Ok(await servicio.CajasDisponibles(ct));

    private int? UsuarioActual() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null;

    [HttpPost("abrir")]
    public async Task<IActionResult> AbrirCaja([FromBody] AperturaCajaDTOs dto, CancellationToken ct)
    {
        if (UsuarioActual() is not int id) return Unauthorized();
        var sesion = await servicio.AbrirCaja(dto, id, ct);
        return Ok(new { mensaje = "Caja abierta correctamente.", id_sesion_caja = sesion.id_sesion_caja, sesion.monto_inicial });
    }

    [HttpPost("cerrar")]
    public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaDTOs dto, CancellationToken ct)
    {
        if (UsuarioActual() is not int id) return Unauthorized();
        var sesion = await servicio.CerrarCaja(dto, id, ct);
        return Ok(new { mensaje = "Caja cerrada correctamente.", sesion.id_sesion_caja,
            sesion.id_usuario_apertura, sesion.id_usuario_cierre, sesion.fecha_apertura, sesion.fecha_cierre,
            sesion.monto_inicial, sesion.monto_esperado, sesion.monto_contado, sesion.diferencia });
    }

    [HttpGet("actual")]
    public async Task<IActionResult> Actual(CancellationToken ct)
    {
        // Consulta administrativa del turno; no abre una caja ni cambia sus datos.
        var actual = await servicio.Actual(ct);
        return actual is null ? NoContent() : Ok(actual);
    }

    [HttpGet("ListarSesionesCaja/{idCaja}")]
    public async Task<IActionResult> ListarSesionesCaja(int idCaja, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50, CancellationToken ct = default)
        => Ok(await servicio.ListarSesionesCaja(idCaja, pagina, tamanoPagina, ct));

    [HttpGet("ListarSesionesCaja")]
    public async Task<IActionResult> ListarSesionesCaja([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50, CancellationToken ct = default)
        => Ok(await servicio.ListarSesionesCaja(null, pagina, tamanoPagina, ct));
}
