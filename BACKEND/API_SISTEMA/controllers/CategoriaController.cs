using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_SISTEMA.Utilidades;
using System.ComponentModel.Design;


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

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        //listar los datos
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var listar = await _Service.ListarCategoria();
            return Ok(listar);
        }


        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("ListarPorCategoria{id}")]
       public async Task<IActionResult> ListarProductoPorCategoria(int id) 
       {
            try
            {
                var listar = await _Service.ListarCatalogoPorCategoria(id);
                return Ok(listar);
            }
            catch (Exception ex) 
            {
                return BadRequest(new

                {
                    mensaje = "Ocurrio un error al listar los productos",
                    detalle = ex.Message
                });
            }
           
       }

    }
}
