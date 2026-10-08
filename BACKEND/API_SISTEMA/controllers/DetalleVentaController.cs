using API_SISTEMA.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers
{
    [Route("api/DetalleVenta_")]
    [ApiController]
    public class DetalleVentaController : ControllerBase
    {
        private readonly DetalleVentaService _Service;

        public DetalleVentaController(DetalleVentaService service)
        {
            _Service = service;
        }


        [HttpGet("listar/{idVenta}")]
        public async Task<IActionResult> ListarDetalle(int idVenta)
        {
            var detalle = await _Service.ListarDetalleVenta(idVenta);

            return Ok(detalle);
        }
    }
}
