using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {

        private readonly RolService _Service;

        public RolController(RolService service)
        {
            _Service = service;
        }
        //listar los datos
        [HttpGet]
        public async Task<IActionResult> Listarol()
        {
            var listar = await _Service.ListarRol();
            return Ok(listar);
        }

        //CREAR UN NUEVO ROL
        [HttpPost]
        public async Task<IActionResult> Crear(Rol rol)
        {
            

            try
            {
                var NuevoRol = await _Service.CrearRol(rol);
                return Ok(NuevoRol);

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
