using API_SISTEMA.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers
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

    }
}
