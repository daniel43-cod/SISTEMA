using System.ComponentModel.DataAnnotations;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.services.ProductoS;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.RateLimiting;


namespace API_SISTEMA.controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ILogger<ProductosController> _logger;
        private readonly BuscarCodigoBarraService _productoService;
        private readonly ProductoService _Service;
        private readonly ProductoCrearService _crearService;
        private readonly SubirImagenService _subirImagenService;


        public ProductosController(ProductoService service, ProductoCrearService crearService, SubirImagenService subirImagenService, BuscarCodigoBarraService productoService, ILogger<ProductosController> logger)
        {
            _logger = logger;
            _Service = service;
            _crearService = crearService;
            _subirImagenService = subirImagenService;
            _productoService = productoService;
        }


        [Authorize(Roles = Roles.Administrador)]
        [HttpGet("listar")]
        [EnableRateLimiting("consulta-productos")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ProductoPaginaDTO>> ListarProductos(
            [FromQuery] ProductoConsultaDTO filtro, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _Service.ListarAdministracion(filtro, cancellationToken));
            }
            catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al listar productos. Referencia: {TraceId}", traceId);
                return StatusCode(500, new { mensaje = "No se pudieron listar los productos.", traceId });
            }
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpGet("{id:int}")]
        [EnableRateLimiting("consulta-productos")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ProductoDetalleDTO>> DetalleProducto(
            [Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
        {
            try
            {
                var producto = await _Service.ObtenerDetalleAdministracion(id, cancellationToken);
                if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
                return Ok(producto);
            }
            catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al consultar producto. Referencia: {TraceId}", traceId);
                return StatusCode(500, new { mensaje = "No se pudo consultar el producto.", traceId });
            }
        }
        [Authorize(Roles = Roles.Administrador)]
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ProductoResumenDTO>> ActualizarProducto(
            [Range(1, int.MaxValue)] int id, [FromBody] ActualizarProductoDTO dto,
            [FromServices] ProductoActualizarService actualizarService, CancellationToken cancellationToken)
        {
            try
            {
                var producto = await actualizarService.ActualizarProducto(id, dto, cancellationToken);
                if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
                return Ok(producto);
            }
            catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (ProductoDuplicadoException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al actualizar producto. Referencia: {TraceId}", traceId);
                return StatusCode(500, new { mensaje = "No se pudo actualizar el producto.", traceId });
            }
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

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost("crear")]
        [Consumes("application/json")]
        public Task<IActionResult> CrearProductos([FromBody] productocrear dto, CancellationToken cancellationToken)
            => CrearInterno(dto, null, cancellationToken);

        // Archivo o foto: el navegador envía multipart con presentaciones[0].id_presentacion, etc.
        [Authorize(Roles = Roles.Administrador)]
        [HttpPost("crear/con-imagen")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        public Task<IActionResult> CrearConImagen([FromForm] CrearProductoConImagenDTO dto,
            CancellationToken cancellationToken)
            => CrearInterno(dto, dto.Imagen, cancellationToken);

        private async Task<IActionResult> CrearInterno(productocrear dto, IFormFile? imagen,
            CancellationToken cancellationToken)
        {
            try
            {
                var producto = await _crearService.CrearProducto(dto, cancellationToken, imagen);
                // No serializar navegaciones de EF ni devolver ciclos de entidades.
                return StatusCode(StatusCodes.Status201Created, new
                {
                    id_producto = producto.id_producto, producto.codigo_barra, producto.nombre,
                    producto.IdMarca, producto.stock_minimo, producto.imagen, producto.fecha_creacion,
                    presentaciones = producto.ProductoPresentaciones.Select(p => new
                    {
                        p.id_producto_presentacion, id_presentacion = p.IdPresentacion,
                        p.unidades_equivalentes, p.precio
                    }),
                    mensaje = "Producto creado correctamente"
                });
            }
            catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (ProductoDuplicadoException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al crear producto. Referencia: {TraceId}", traceId);
                return StatusCode(500, new { mensaje = "No se pudo crear el producto.", traceId });
            }
        }
        [Authorize(Roles =Roles.Administrador)]
        [HttpPost("{id}/imagen")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
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


