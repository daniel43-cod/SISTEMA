using System.Security.Claims;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Services.Fel;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/Venta/{idVenta:int:min(1)}")]
[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
[EnableRateLimiting("ventas")]
[TypeFilter(typeof(VentaExceptionFilter))]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class FacturaFelController(CertificarVentaService servicio) : ControllerBase
{
    private int Usuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

    [HttpPost("certificar")]
    public async Task<IActionResult> Certificar(int idVenta, CertificarVentaDto dto, CancellationToken ct)
    {
        try { return Ok(await servicio.Certificar(idVenta, dto, Usuario(), ct)); }
        catch (ConsultaReceptorNoDisponibleException)
        { return StatusCode(502, new { mensaje = "No se pudo verificar el receptor en INFILE. La venta sigue registrada; vuelve a consultar el documento." }); }
    }

    [HttpGet("fel/preparacion")]
    public async Task<IActionResult> Preparar(int idVenta, CancellationToken ct)
        => Ok(await servicio.Preparar(idVenta, Usuario(), ct));

    [HttpGet("fel")]
    public async Task<IActionResult> Consultar(int idVenta, CancellationToken ct)
        => Ok(new { documento = await servicio.Consultar(idVenta, Usuario(), ct) });

    [HttpPut("receptor")]
    public async Task<IActionResult> EditarReceptor(int idVenta, EditarReceptorVentaDto dto,
        [FromServices] API_SISTEMA.Services.Ventas.EditarReceptorVentaService edicion, CancellationToken ct)
        => Ok(await edicion.Editar(idVenta, dto, Usuario(), ct));
}
