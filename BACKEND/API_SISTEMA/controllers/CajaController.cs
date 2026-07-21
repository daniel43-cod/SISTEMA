using API_SISTEMA.DTOs.Caja;
using API_SISTEMA.services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CajaController : ControllerBase
    {
        
        private readonly CajaService _cajaService;

        public CajaController(CajaService cajaService)
        {
            _cajaService = cajaService;
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AperturaCajaDTOs caja)
        {
            

            try
            {
                var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(idUsuarioClaim, out int idUsuario))
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario autenticado."
                    });

                var sesionCaja = await _cajaService
                    .AbrirCaja(caja, idUsuario);

                return Ok(new
                {
                    mensaje = "Caja abierta correctamente.",
                   /* id_sesion_caja = sesionCaja.id_sesion_caja,
                    id_caja = sesionCaja.id_caja,
                    id_usuario_apertura =
                        sesionCaja.id_usuario_apertura,
                    fecha_apertura = sesionCaja.fecha_apertura,
                    monto_inicial = sesionCaja.monto_inicial*/
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
        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaDTOs dto)
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
                        mensaje =
                            "No se pudo identificar al usuario autenticado."
                    });
                }

                var sesion = await _cajaService
                    .CerrarCaja(dto, idUsuario);

                return Ok(new
                {
                    mensaje = "Caja cerrada correctamente.",
                    id_sesion_caja = sesion.id_sesion_caja,
                    fecha_apertura = sesion.fecha_apertura,
                    fecha_cierre = sesion.fecha_cierre,
                    monto_inicial = sesion.monto_inicial,
                    monto_esperado = sesion.monto_esperado,
                    monto_contado = sesion.monto_contado,
                    diferencia = sesion.diferencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
