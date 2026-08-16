using API_SISTEMA.DTOs.Gastos;
using API_SISTEMA.services.Gastos;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace API_SISTEMA.controllers
{
    public class GastosController : Controller
    {
        private readonly CrearGastosService _service;
        public GastosController(CrearGastosService service)
        {
            _service = service;
        }

        [Authorize(Roles =Roles.Administrador)]
        [HttpPost("CrearGasto")]
        public async Task<IActionResult> CrearGasto(IngresarGastoDTOs gastoDto)
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

                var idGasto = await _service.CrearGasto(
                    gastoDto,
                    idUsuario
                );

                return Ok(new
                {
                    mensaje = "Gasto registrado correctamente.",
                    id_gasto = idGasto
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
