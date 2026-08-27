using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.ProductoS;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.ComponentModel.Design;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly BuscarCodigoBarraService _productoService;
        private readonly ProductoService _Service;
        private readonly ProductoCrearService _crearService;
        private readonly SubirImagenService _subirImagenService;
        //private readonly productocrear productocrear;
        // private readonly ProductoPrecioService productoprecio;

        public ProductosController(ProductoService service, ProductoCrearService crearService, SubirImagenService subirImagenService, BuscarCodigoBarraService productoService)
        {
            _Service = service;
            _crearService = crearService;
            _subirImagenService = subirImagenService;
            _productoService = productoService;
        }
        [Authorize(Roles ="ADMINISTRADOR")]
        [HttpGet("listar")]
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
        [HttpPost("crear")]
        public async Task<IActionResult> CrearProductos([FromBody] productocrear dto)
        {
            try
            {
                var producto = await _crearService.CrearProducto(dto);

                return Ok(new
                {
                    id_producto = producto.id_producto,
                    mensaje = "Producto creado correctamente"
                });
            }
            catch (Exception ex)
            {
                {
                    return BadRequest(new
                    {
                        mensaje = ex.Message,
                        detalle = ex.ToString()
                    });
                }
            }
        }

        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> SubirImagen(int id, IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest("Debe subir una imagen.");

            var producto = await _subirImagenService.SubirImagen(id, imagen);

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


        [HttpGet("codigo/{codigoBarra}")]
        public async Task<IActionResult> BuscarPorCodigoBarra(string codigoBarra)
        {
            try
            {
                var productos =
                    await _productoService
                        .BuscarPorCodigoBarra(codigoBarra);

                if (productos == null   )
                {
                    return NotFound(new
                    {
                        mensaje =
                            "No existe un producto con ese código de barras."
                    });
                }

                return Ok(productos);
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
