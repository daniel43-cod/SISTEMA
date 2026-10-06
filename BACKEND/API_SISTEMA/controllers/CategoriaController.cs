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
        private readonly CategoriaActualizarService _actualizarService;
        private readonly CategoriaListarService _listarService;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(CategoriaService service, CategoriaCrearService crearService,
            ILogger<CategoriaController> logger, CategoriaActualizarService actualizarService,
            CategoriaListarService listarService)
        {
            _Service = service;
            _crearService = crearService;
            _actualizarService = actualizarService;
            _listarService = listarService;
            _logger = logger;
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet]
        public async Task<ActionResult<List<RespuestaCategoriaDTO>>> Listar(CancellationToken cancellationToken)
        {
            try
            {
                // Si no hay categorías activas, devuelve 200 con una lista vacía.
                return Ok(await _listarService.ListarActivas(cancellationToken));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al listar categorías. Referencia: {TraceId}", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "No se pudieron listar las categorías.", traceId
                });
            }
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPatch("{idCategoria:int}/estado")]
        [Consumes("application/json")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> CambiarEstado([Range(1, int.MaxValue)] int idCategoria,
            [FromBody] CambiarEstadoCategoriaDTO dto, [FromServices] EstadoCategoriaService service,
            CancellationToken cancellationToken)
        {
            try
            {
                var resultado = await service.CambiarEstado(idCategoria, dto, cancellationToken);
                if (resultado is null) return NotFound(new { mensaje = "La categoría no existe." });
                return Ok(resultado);
            }
            catch (ValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al cambiar estado de categoría. Referencia: {TraceId}", traceId);
                return StatusCode(500, new { mensaje = "No se pudo cambiar el estado de la categoría.", traceId });
            }
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpGet("administracion")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ListarAdministracion(CancellationToken cancellationToken)
        {
            try { return Ok(await _listarService.ListarAdministracion(cancellationToken)); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al listar categorías para administración. Referencia: {TraceId}", traceId);
                return StatusCode(500, new { mensaje = "No se pudieron listar las categorías.", traceId });
            }
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

        [Authorize(Roles = Roles.Administrador)]
        [HttpPut("{idCategoria:int}")]
        [Consumes("application/json")]
        public Task<IActionResult> Actualizar(int idCategoria, [FromBody] ActualizarCategoriaDTO dto,
            CancellationToken cancellationToken)
            => ActualizarInterno(idCategoria, dto, null, cancellationToken);

        [Authorize(Roles = Roles.Administrador)]
        [HttpPut("{idCategoria:int}/con-imagen")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        public Task<IActionResult> ActualizarConImagen(int idCategoria,
            [FromForm] ActualizarCategoriaConImagenDTO dto, CancellationToken cancellationToken)
            => ActualizarInterno(idCategoria, dto, dto.Imagen, cancellationToken);

        // Ambos formatos comparten el servicio y las mismas respuestas de error.
        private async Task<IActionResult> ActualizarInterno(int idCategoria, ActualizarCategoriaDTO dto,
            IFormFile? imagen, CancellationToken cancellationToken)
        {
            try
            {
                var categoria = await _actualizarService.ActualizarCategoria(
                    idCategoria, dto, cancellationToken, imagen);
                if (categoria is null)
                    return NotFound(new { mensaje = "La categoría no existe." });
                return Ok(categoria);
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
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error al actualizar categoría. Referencia: {TraceId}", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "No se pudo actualizar la categoría.", traceId
                });
            }
        }

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


