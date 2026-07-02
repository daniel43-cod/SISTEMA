using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisoController : ControllerBase
    {

        private readonly PermisoService _permisoService;

        public PermisoController(PermisoService permisoService)
        {
            _permisoService = permisoService;
        }

        // Trae todos los permisos que tiene un rol (para mostrar checkboxes marcados en WPF)
        [HttpGet("rol/{idRol}")]
        public async Task<IActionResult> ObtenerPermisosDeRol(int idRol)
        {
            var permisos = await _permisoService.ObtenerPermisosDeRol(idRol);
            return Ok(permisos);
        }

        // El admin marca una casilla -> se asigna el permiso
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

        // El admin desmarca una casilla -> se quita el permiso
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
