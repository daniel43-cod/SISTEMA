using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using API_SISTEMA.Utilidades;
using API_SISTEMA.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers
{
    [Route("api/DetalleVenta_")]
    [ApiController]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
    [TypeFilter(typeof(VentaExceptionFilter))]
    public class DetalleVentaController : ControllerBase
    {
        private int Usuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : throw new UnauthorizedAccessException("No se pudo identificar al usuario.");
        private readonly DetalleVentaService _Service;

        public DetalleVentaController(DetalleVentaService service)
        {
            _Service = service;
        }


        [HttpGet("listar/{idVenta}")]
        public async Task<IActionResult> ListarDetalle(int idVenta, CancellationToken ct)
        {
            var detalle = await _Service.ListarDetalleVenta(idVenta, Usuario(), ct);

            return Ok(detalle);
        }
    }
}
