using API_SISTEMA.DTOs.EmpresaDTOs;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    public class EmpresaController : Controller
    {
        private readonly EmpresaService _empresaService;

        public EmpresaController(EmpresaService empresaService)
        {
            _empresaService = empresaService;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] EmpresaDTOs empresaDto)
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

    }
}
