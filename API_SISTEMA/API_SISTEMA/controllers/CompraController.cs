using API_SISTEMA.DTOs.Compras;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        private readonly CompraService _context;

        public CompraController(CompraService service)
        {
            _context = service;
        }


        [Authorize(Roles = Roles.Administrador)]

        [HttpGet("listar")]
        public async Task<IActionResult> listar()
        {
           var compras = await _context.listarcompras();
            return Ok(compras);
        }

        [Authorize(Roles = Roles.Administrador)]

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] RegistroComprasDTO compraDto)
        {
            try
            {
                var compra = await _context.CrearCompra(compraDto);

                return Ok(new
                {
                    mensaje = "Venta registrada correctamente",
                   
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }
        }

        [Authorize(Roles = Roles.Administrador)]
        //listar detallecompras
        [HttpGet("detalle/{id_compra}")] 
        public async Task<IActionResult> ListarDetalleCompra(int id_compra)
        {
            try
            {
                var detalleCompra = await _context.ListarDetalleCompra(id_compra);
                return Ok(detalleCompra);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }
        }

    }
}
