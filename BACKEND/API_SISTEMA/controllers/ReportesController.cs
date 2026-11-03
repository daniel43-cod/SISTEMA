using System.Security.Claims;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = Roles.Administrador)]
[EnableRateLimiting("ventas")]
[TypeFilter(typeof(VentaExceptionFilter))]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ReportesController(ReporteVentasIntervaloService reporte) : ControllerBase
{
    [HttpGet("ventas-acumuladas")]
    public async Task<ActionResult<ReporteVentasAcumuladasDto>> VentasAcumuladas(
        [FromQuery] int idSesionCaja, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario) || idUsuario <= 0)
            return Unauthorized();
        return Ok(await reporte.ConsultarAcumulado(idUsuario, idSesionCaja, ct));
    }

    [HttpGet("ventas-por-intervalo")]
    public async Task<ActionResult<ReporteVentasIntervaloDto>> VentasPorIntervalo(
        [FromQuery] FiltroReporteVentasIntervaloDto filtro, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario) || idUsuario <= 0)
            return Unauthorized();
        return Ok(await reporte.Consultar(idUsuario, filtro, ct));
    }
}
