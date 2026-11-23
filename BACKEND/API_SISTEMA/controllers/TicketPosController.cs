using System.Security.Claims;
using API_SISTEMA.Services.Comprobantes;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[ApiController, Route("api/comprobantes/ventas/{idVenta:int:min(1)}/ticket-pos")]
[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
[EnableRateLimiting("ventas"), TypeFilter(typeof(VentaExceptionFilter))]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class TicketPosController(TicketPosService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TicketPosDto>> Obtener(int idVenta, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuario) || usuario <= 0) return Unauthorized();
        return Ok(await servicio.Obtener(idVenta, usuario, ct));
    }
}
