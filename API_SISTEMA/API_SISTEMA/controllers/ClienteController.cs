using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
        [Route("api/[controller]")]
        [ApiController]
        public class ClienteController : ControllerBase
        {

            private readonly ClienteService _Service;

            public ClienteController(ClienteService service)
            {
                _Service = service;
            }

        [HttpGet]
        public async Task<IActionResult> listar()
        {
            var listar = await _Service.ListarCliente();
            return Ok(listar);
        }


   
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string texto)
        {
            try
            {
                var clientes = await _Service.BuscarClientes(texto);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

    }   
}
