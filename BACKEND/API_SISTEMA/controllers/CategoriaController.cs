using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using API_SISTEMA.services.Categoria;
using API_SISTEMA.DTOs.Categoria;
using System.ComponentModel.DataAnnotations;
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
        private readonly CategoriaCrearService _crearService;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(CategoriaService service, CategoriaCrearService crearService,
            ILogger<CategoriaController> logger)
        {
            _Service = service;
            _crearService = crearService;
            _logger = logger;
        }

        // El permiso se comprueba en el servidor, no solo en el menú del frontend.
        [Authorize(Roles = Roles.Administrador)]
        [HttpPost]
        [Consumes("application/json")]
        public Task<IActionResult> Crear([FromBody] CrearCategoriaDTO dto, CancellationToken cancellationToken)
            => CrearInterno(dto, null, cancellationToken);

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost("con-imagen")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        public Task<IActionResult> CrearConImagen([FromForm] CrearCategoriaConImagenDTO dto, CancellationToken cancellationToken)
            => CrearInterno(dto, dto.Imagen, cancellationToken);

        private async Task<IActionResult> CrearInterno(CrearCategoriaDTO dto, IFormFile? imagen, CancellationToken cancellationToken)
        {
            try
            {
                var categoria = await _crearService.CrearCategoria(dto, cancellationToken, imagen);
                return StatusCode(StatusCodes.Status201Created, categoria);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (CategoriaDuplicadaException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Registramos el detalle técnico sin exponerlo al navegador.
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al crear categoría. Referencia: {TraceId}", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "No se pudo crear la categoría. Inténtalo de nuevo.", traceId
                });
            }
        }

       
        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("ListarPorCategoria/{id}")]
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


