using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Categoria;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Categoria;

public class CategoriaListarService
{
    private readonly SistemaDbContext _context;

    public CategoriaListarService(SistemaDbContext context)
    {
        _context = context;
    }

    // Consulta de solo lectura: devuelve las categorías activas ordenadas por nombre.
    public Task<List<RespuestaCategoriaDto>> ListarAdministracion(CancellationToken cancellationToken = default)
        => _context.categorias.AsNoTracking()
            .OrderBy(c => c.nombreCategoria).ThenBy(c => c.IdCategoria)
            .Select(c => new RespuestaCategoriaDto
            {
                IdCategoria = c.IdCategoria, Nombre = c.nombreCategoria,
                Estado = c.Estado, UrlImagen = c.UrlImagen
            }).ToListAsync(cancellationToken);

    public Task<List<RespuestaCategoriaDto>> ListarActivas(CancellationToken cancellationToken = default)
    {
        return _context.categorias.AsNoTracking()
            .Where(c => c.Estado)
            .OrderBy(c => c.nombreCategoria).ThenBy(c => c.IdCategoria)
            .Select(c => new RespuestaCategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.nombreCategoria,
                Estado = c.Estado,
                UrlImagen = c.UrlImagen
            })
            .ToListAsync(cancellationToken);
    }
}
