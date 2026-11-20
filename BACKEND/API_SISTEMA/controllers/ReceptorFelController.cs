using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API_SISTEMA.Services.Fel;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

public sealed class ConsultarReceptorDto
{
    [Required, RegularExpression("^(NIT|CUI)$")] public string tipo { get; set; } = "";
    [Required, StringLength(20)] public string identificacion { get; set; } = "";
}

[ApiController, Route("api/Fel/receptor")]
[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
[EnableRateLimiting("ventas")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[TypeFilter(typeof(VentaExceptionFilter))]
public sealed class ReceptorFelController(ConsultarReceptorInfileService servicio, PermisoUsuarioService permisos) : ControllerBase
{
    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar(ConsultarReceptorDto dto, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuario) ||
            !await permisos.TienePermiso(usuario, Utilidades.Permisos.Vender))
            return Forbid();
        try { return Ok(await servicio.Consultar(dto.tipo, dto.identificacion, ct)); }
        catch (ConsultaReceptorNoDisponibleException)
        {
            return StatusCode(502, new { mensaje = "No se pudo consultar el receptor en INFILE. Intenta de nuevo o consulta a soporte." });
        }
    }
}
