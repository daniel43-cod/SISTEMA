using System.Security.Claims;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.Gastos;
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
public sealed class ReportesController(ReporteVentasIntervaloService reporte, ReporteComprasService reporteCompras, ReporteGastosService reporteGastos) : ControllerBase
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

    [HttpGet("compras-caja-activa")]
public async Task<ActionResult<ReporteComprasDto>> ComprasCajaActiva(
    CancellationToken ct)
{
    if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var idUsuario) ||
        idUsuario <= 0)
    {
        return Unauthorized();
    }

    var resultado = await reporteCompras.ConsultarCajaActiva(
        idUsuario,
        ct);

    if (resultado is null)
    {
        return NoContent();
    }

    return Ok(resultado);
}

[HttpGet("gastos-caja-activa")]
public async Task<ActionResult<ReporteGastosDto>> GastosCajaActiva(
    CancellationToken ct)
{
    if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var idUsuario) ||
        idUsuario <= 0)
    {
        return Unauthorized();
    }

    var resultado = await reporteGastos.ConsultarCajaActiva(
        idUsuario,
        ct);

    if (resultado is null)
    {
        return NoContent();
    }

    return Ok(resultado);
}
}
