using API_SISTEMA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using API_SISTEMA.Utilidades;
using API_SISTEMA.Dtos.Pagos;

namespace API_SISTEMA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
    [TypeFilter(typeof(VentaExceptionFilter))]
    public class PagosController : ControllerBase
    {
            private readonly PagoService _service;

            public PagosController(PagoService service)
            {
                _service = service;
            }

        [HttpGet("venta/{idVenta:int}")]
        [EnableRateLimiting("ventas")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(List<PagoVentaHistorialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListarPorVenta(int idVenta, CancellationToken ct = default)
            => Ok(await _service.ListarPorVenta(idVenta, ct));

           


    }
}
