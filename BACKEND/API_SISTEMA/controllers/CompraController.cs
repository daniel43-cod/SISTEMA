using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Services;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.PagoCompra;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_SISTEMA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("compras")]
    [RequestSizeLimit(131072)]
    [TypeFilter(typeof(CompraExceptionFilter))]
    public class CompraController : ControllerBase
    {
        private readonly CompraService _context;
        private readonly CrearCompraService _crearCompraService;
        private readonly PagoCompraService _pagoCompraService;


        public CompraController(CompraService service, PagoCompraService pagoCompraService, CrearCompraService crearCompraService)
        {
            _context = service;
            _pagoCompraService = pagoCompraService;
            _crearCompraService = crearCompraService;
        }


        [Authorize(Roles = Roles.Administrador)]

        [HttpGet("listar")]
        public async Task<IActionResult> listar([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50, CancellationToken ct = default)
        {
           var compras = await _context.listarcompras(pagina, tamanoPagina, ct);
            return Ok(compras);
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] RegistroComprasDto compraDto, CancellationToken ct)
        {
                var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario) || idUsuario <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var compra = await _crearCompraService.CrearCompra(compraDto, idUsuario, ct);

                return Ok(new
                {
                    mensaje = "Compra registrada correctamente",
                    id_compra = compra.IdCompra,
                   
                });

        }

        [Authorize(Roles = Roles.Administrador)]
        //listar detallecompras
        [HttpGet("detalle/{id_compra}")] 
        public async Task<IActionResult> ListarDetalleCompra(int id_compra, CancellationToken ct)
        {
                var detalleCompra = await _context.ListarDetalleCompra(id_compra, ct);
                return detalleCompra is null ? NotFound(new { mensaje = "La compra no existe." }) : Ok(detalleCompra);

               

        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost("pago-compra")]
        public async Task<IActionResult> RegistrarPagoCompra([FromBody] AbonarSaldoCompraDto dto, CancellationToken ct)
        {
                var idUsuarioClaim =User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario) || idUsuario <= 0)
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var pago = await _pagoCompraService.AbonarCompra(dto, idUsuario, ct);

                return Ok(new
                {
                    mensaje = "Pago registrado correctamente.",
                    id_pago = pago.id_pagos_compra,
                    id_compra = pago.id_compra,
                    monto_pagado = pago.monto,
                    fecha_pago = pago.fecha_pago
                });

        }

    }
}
