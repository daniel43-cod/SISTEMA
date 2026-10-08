using API_SISTEMA.Dtos.Empresa;
using API_SISTEMA.Services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers
{
    [Authorize(Roles =Roles.Administrador)]
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : Controller
    {
        private readonly EmpresaService _empresaService;

        public EmpresaController(EmpresaService empresaService)
        {
            _empresaService = empresaService;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] EmpresaDto empresaDto)
        {
            try
            {
                var empresa = await _empresaService.CrearEmpresa(empresaDto);

                return Ok(new
                {
                    mensaje = "Empresa registrada correctamente",
                   
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

        [HttpGet("listar")]
        public async Task<IActionResult> Listar()
        {
            var empresas = await _empresaService.ListarEmpresa();
            return Ok(empresas);
        }

    }
}
