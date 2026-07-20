using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ListarUsuario()
        {
            var listar = await _service.ListarUsuario();
            return Ok(listar);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Usuario usuario)
        {
            try
            {
                var NuevoUsuario = await _service.CrearUsuario(usuario);
                return Ok(NuevoUsuario);

            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
