using API_SISTEMA.services.Auditoria;
using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Categoria;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Categoria
{
    public class CategoriaCrearService
    {
        // El contenedor de dependencias proporciona el contexto de esta petición.
        private readonly CatalogoAuditoriaService _auditoria;
    private readonly SistemaDbContext _context;
        private readonly CategoriaImagenService _imagenes;

        public CategoriaCrearService(SistemaDbContext context, CategoriaImagenService imagenes, CatalogoAuditoriaService auditoria)
        {
            _auditoria = auditoria;
        _context = context;
            _imagenes = imagenes;
        }

        public async Task<RespuestaCategoriaDTO> CrearCategoria(
            CrearCategoriaDTO dto, CancellationToken cancellationToken = default, IFormFile? imagen = null)
        {
            // Validar también aquí protege llamadas al servicio fuera del controlador.
            if (dto is null)
                throw new ValidationException("La información de la categoría es obligatoria.");
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

            var url = _imagenes.ValidarUrl(dto.UrlImagen);
            if (url != null && imagen != null)
                throw new ValidationException("Elige un enlace o un archivo, no ambos.");
            string? archivoGuardado = null;
            bool commitIniciado = false;
            try
            {
                if (imagen != null) url = archivoGuardado = await _imagenes.GuardarAsync(imagen, cancellationToken);
    
                // Guardamos el nombre limpio y comparamos sin distinguir mayúsculas.
                var nombreCategoria = dto.Nombre.Trim();
                var nombreNormalizado = nombreCategoria.ToUpperInvariant();
    
                // La comprobación y el alta son una sola operación transaccional.
                // Serializable impide que dos altas concurrentes confirmen el mismo nombre.
                // Si hay una excepción, DisposeAsync revierte la transacción no confirmada.
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
    
                // Incluimos categorías inactivas para no duplicar una categoría existente.
                var existe = await _context.categorias.AnyAsync(c => c.nombreCategoria != null &&
                    c.nombreCategoria.Trim().ToUpper() == nombreNormalizado, cancellationToken);
                if (existe)
                    throw new CategoriaDuplicadaException();
    
                // El servidor asigna estado y fecha; el ID lo genera SQL Server.
                var categoria = new API_SISTEMA.models.Categoria
                {
                    nombreCategoria = nombreCategoria,
                    FechaCreacion = DateTime.Now,
                    Estado = true,
                    UrlImagen = url
                };
            _context.categorias.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditoria.Agregar("CATEGORIA_CREADA", "categorias", categoria.IdCategoria, null,
                new Dictionary<string, object?> { ["nombre"] = categoria.nombreCategoria, ["urlImagen"] = categoria.UrlImagen, ["estado"] = categoria.Estado }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            // Si falla la confirmación, su resultado podría ser incierto: conservar el archivo.
            commitIniciado = true;
            await transaction.CommitAsync(cancellationToken);

            // Devolvemos un DTO, sin exponer la entidad de Entity Framework.
            return new RespuestaCategoriaDTO
            {
                IdCategoria = categoria.IdCategoria,
                Nombre = categoria.nombreCategoria,
                Estado = categoria.Estado,
                UrlImagen = categoria.UrlImagen
            };
            }
            catch
            {
                if (archivoGuardado != null && !commitIniciado) _imagenes.Eliminar(archivoGuardado);
                throw;
            }
        }
    }
}
