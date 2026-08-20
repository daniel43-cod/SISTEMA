using API_SISTEMA.data;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_SISTEMA.controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {

        private readonly VentaService _context;
        private readonly CrearVentaService _crearVentaService;
        private readonly AbonarSaldoVentaServices _pagoService;

        public VentaController(VentaService service, AbonarSaldoVentaServices pago, CrearVentaService crearVenta)
        {
            _context = service;
            _pagoService = pago;
            _crearVentaService = crearVenta;
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("listar")]
        public async Task<IActionResult> ListarVentas([FromQuery] FiltrarVentasDTOs filtro)
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

                var rol =
                    User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrWhiteSpace(rol))
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar el rol del usuario."
                    });
                }

                var listar = await _context.ListarVentas(idUsuario);

                return Ok(listar);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = "Ocurrió un error al listar las ventas.",
                    detalle = ex.Message
                });
            }
        }

        //catalogo
        [Authorize(Roles =Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("catalogo")]
        public async Task<IActionResult> ListarCatalogo()
        {
            try
            {
                var catalogo = await _context.ListarCatalogo();

                return Ok(catalogo);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje1 = "A ocurrido un error al cargar la imagen",
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }

        }


        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpPost("AbonarVenta")]
        public async Task<IActionResult> AbonarVenta( [FromBody] AbonarSaldoVentaDTO dto)
        {
            try
            {
                var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario))
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var pago = await _pagoService.AbonarVenta(dto,idUsuario);

                return Ok(new
                {
                    mensaje = "Abono registrado correctamente.",
                    id_pago = pago.id_pago,
                    id_venta = pago.id_venta,
                    monto = pago.monto,
                    fecha_pago = pago.fecha_pago,
                    id_sesion_caja = pago.id_sesion_caja
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


        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear(
    [FromBody] CrearVentaDTO ventaDto)
        {
            try
            {
                var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue("sub");

                if (!int.TryParse(idUsuarioClaim, out int idUsuario))
                {
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });
                }

                var venta = await _crearVentaService.CrearVenta(
                    ventaDto,
                    idUsuario
                );

                return Ok(new
                {
                    mensaje = "Venta registrada correctamente.",
                   
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
