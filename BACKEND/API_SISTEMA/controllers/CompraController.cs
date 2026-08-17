using API_SISTEMA.DTOs.Compras;
using API_SISTEMA.services;
using API_SISTEMA.services.PagoCompra;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        private readonly CompraService _context;
        private readonly Pago _pagoCompraService;


        public CompraController(CompraService service, Pago pagoCompraService)
        {
            _context = service;
            _pagoCompraService = pagoCompraService;
        }


        [Authorize(Roles = Roles.Administrador)]

        [HttpGet("listar")]
        public async Task<IActionResult> listar()
        {
           var compras = await _context.listarcompras();
            return Ok(compras);
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] RegistroComprasDTO compraDto)
        {
            try
            {
                var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario))
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var compra = await _context.CrearCompra(compraDto, idUsuario);

                return Ok(new
                {
                    mensaje = "Compra registrada correctamente",
                   
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }
        }

        [Authorize(Roles = Roles.Administrador)]
        //listar detallecompras
        [HttpGet("detalle/{id_compra}")] 
        public async Task<IActionResult> ListarDetalleCompra(int id_compra)
        {
            try
            {
                var detalleCompra = await _context.ListarDetalleCompra(id_compra);
                return Ok(detalleCompra);

               
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpPost("pago-compra")]
        public async Task<IActionResult> RegistrarPagoCompra(AbonarSaldoCompraDTO  dto)
        {
            try
            {
                var idUsuarioClaim =User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario))
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var pago = await _pagoCompraService.AbonarCompra(dto, idUsuario);

                return Ok(new
                {
                    mensaje = "Pago registrado correctamente.",
                    id_pago = pago.id_pagos_compra,
                    id_compra = pago.id_compra,
                    monto_pagado = pago.monto,
                    fecha_pago = pago.fecha_pago
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }
        }

    }
}
