using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {

        private readonly ProductoService _Service;
        //private readonly productocrear productocrear;
       // private readonly ProductoPrecioService productoprecio;

        public ProductosController(ProductoService service)
        {
            _Service = service;
        }
        //listar producto

        [HttpGet]
        public async Task<IActionResult> ListarProductos()
        {
            var listar = await _Service.ListarProductos();
            return Ok(listar);
    
        }

      /*  [HttpGet("buscar")]
        public async Task<IActionResult> Buscar(string texto)
        {
            var productos = await _Service.BuscarPorNombre(texto);
            return Ok(productos);

        }*/

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromForm] productocrear productoDto)
        {
            try
            {
                var nuevoProducto = await _Service.CrearProducto(productoDto,productoDto.imagen);

                return Ok(new
                {
                    mensaje = "Producto creado correctamente",
                    id_producto = nuevoProducto.id_producto,
                    nombre = nuevoProducto.nombre
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string texto)
        {
            try
            {
                var productos = await _Service.BuscarProductos(texto);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
