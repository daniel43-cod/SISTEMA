using API_SISTEMA.Services;
using Microsoft.AspNetCore.Mvc;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;

namespace API_SISTEMA.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    [Route("api/[controller]")]
    [ApiController]
    public class PermisoController : ControllerBase
    {

        private readonly PermisoService _permisoService;

        public PermisoController(PermisoService permisoService)
        {
            _permisoService = permisoService;
        }

    
        [HttpGet("rol/{idRol}")]
        public async Task<IActionResult> ObtenerPermisosDeRol(int idRol)
        {
            var permisos = await _permisoService.ObtenerPermisosDeRol(idRol);
            return Ok(permisos);
        }

    
        [HttpPost("asignar")]
        public async Task<IActionResult> Asignar(int idRol, int idPermiso)
        {
            try
            {
                await _permisoService.AsignarPermiso(idRol, idPermiso);
                return Ok(new { mensaje = "Permiso asignado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("quitar")]
        public async Task<IActionResult> Quitar(int idRol, int idPermiso)
        {
            try
            {
                await _permisoService.QuitarPermiso(idRol, idPermiso);
                return Ok(new { mensaje = "Permiso removido correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
