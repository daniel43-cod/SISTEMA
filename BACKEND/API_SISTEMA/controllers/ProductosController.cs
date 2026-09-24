using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.ProductoS;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using API_SISTEMA.Utilidades;


namespace API_SISTEMA.controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        ILogger<ProductosController> _logger;
        private readonly BuscarCodigoBarraService _productoService;
        private readonly ProductoService _Service;
        private readonly ProductoCrearService _crearService;
        private readonly SubirImagenService _subirImagenService;


        public ProductosController(ProductoService service, ProductoCrearService crearService, SubirImagenService subirImagenService, BuscarCodigoBarraService productoService)
        {
            _Service = service;
            _crearService = crearService;
            _subirImagenService = subirImagenService;
            _productoService = productoService;
        }


        [Authorize(Roles =Roles.Administrador)]
        [HttpGet("listar")]
        public async Task<IActionResult> ListarProductos()
        {
            try
            {
                  var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var listar = await _Service.ObtenerTodosProductosVenta();
            return Ok(listar);
            }
           catch (Exception ex){
         _logger.LogError(ex, "Error al listar productos");

         return StatusCode(500, new{
        mensaje = "Ocurrió un error interno. Intentá más tarde." });}
    
        }
        //para presentacion de productos
        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("{id}/presentaciones")]
        public async Task<IActionResult> ListarPresentaciones(int id)
        {
            try
            {
                 var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var presentaciones = await _Service.ListarPresentaciones(id);
            return Ok(presentaciones);
            }
            catch
            {
                return BadRequest(new
                {
                    mensaje = "A ocurrido un error al buscar la presentacion, Intentalo mas tarde o comunicate con el administrador"
                });
            }
        }

        [Authorize(Roles =Roles.Administrador)]
        [HttpPost("crear")]
        public async Task<IActionResult> CrearProductos([FromBody] productocrear dto)
        {
            try
            {
                  var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

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
                        mensaje = "A ocurrido un error al ingresar un nuevo producto, intentalo mas tarde o comunicate con el administrador"
                       // mensaje = ex.Message,
                       // detalle = ex.ToString()
                    });
                }
            }
        }
        [Authorize(Roles =Roles.Administrador)]
        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> SubirImagen(int id, IFormFile imagen)
        {
            try
            {
                 var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
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
            catch
            {
                return BadRequest(new
                {
                    mensaje ="A ocurrido un error, Intentalo mas tarde o comunicate con el adminstrador"
                });
            }
        }

    
        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string texto)
        {
              var idUsuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            try
            {
                var productos = await _Service.BuscarProductosVenta(texto);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                //return BadRequest(new { mensaje = ex.Message });
                return BadRequest(new
                {
                    mensaje = "A ocurrido un error al buscar el producto, Intentalo mas tarde o comunicate con el administrador"
                });
                    
                
            }
        }

      
        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
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
                  mensaje ="A ocurrido un error al buscar el producto, Intentalo mas tade o comunicate con el adminstrador"
                });
            }
        }



    }
}
