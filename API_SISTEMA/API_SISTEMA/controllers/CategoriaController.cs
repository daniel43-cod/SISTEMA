using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {

        private readonly CategoriaService _Service;
        
        public CategoriaController(CategoriaService service)
        {
            _Service = service;
        }
        //listar los datos
        [HttpGet]
        public async Task<IActionResult>Listar()
        {
            var listar=await _Service.ListarCategoria();
            return Ok(listar);
        }
    
    }
}
