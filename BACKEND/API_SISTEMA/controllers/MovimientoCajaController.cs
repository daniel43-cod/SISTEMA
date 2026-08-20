using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace API_SISTEMA.controllers
{
    public class MovimientoCajaController : Controller
    {

        private readonly ListarMovimientoCajaService _service;

        public MovimientoCajaController(ListarMovimientoCajaService service)
        {
            _service = service;
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("listar")]
        public async Task<IActionResult> ListarMovimientos()
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

                var movimientos =
                    await _service.ListarMovimientos(idUsuario);

                return Ok(movimientos);
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
