using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

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
        [Authorize(Roles ="ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> ListarProductos()
        {
            var listar = await _Service.ObtenerTodosProductosVenta();
            return Ok(listar);
    
        }
        //para presentacion de productos
        [HttpGet("{id}/presentaciones")]
        public async Task<IActionResult> ListarPresentaciones(int id)
        {
            var presentaciones = await _Service.ListarPresentaciones(id);
            return Ok(presentaciones);
        }

        [Authorize(Roles ="ADMINISTRADOR")]
        [HttpPost]
        public async Task<IActionResult> CrearProductos([FromBody] productocrear dto)
        {
            var producto = await _Service.CrearProducto(dto);

            return Ok(new
            {
                id_producto = producto.id_producto,
                mensaje = "Producto creado correctamente"
            });
        }

        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> SubirImagen(int id, IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest("Debe subir una imagen.");

            var producto = await _Service.SubirImagen(id, imagen);

            if (producto == null)
                return NotFound("Producto no encontrado.");

            return Ok(new
            {
                mensaje = "Imagen subida correctamente",
                imagen = producto.imagen
            });
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string texto)
        {
            try
            {
                var productos = await _Service.BuscarProductosVenta(texto);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
