using API_SISTEMA.data;
using API_SISTEMA.DTOs.Categoria;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Categoria;

public class CategoriaListarService
{
    private readonly SistemaDbContext _context;

    public CategoriaListarService(SistemaDbContext context)
    {
        _context = context;
    }

    // Consulta de solo lectura: devuelve las categorías activas ordenadas por nombre.
    public Task<List<RespuestaCategoriaDTO>> ListarActivas(CancellationToken cancellationToken = default)
    {
        return _context.categorias.AsNoTracking()
            .Where(c => c.Estado)
            .OrderBy(c => c.nombreCategoria).ThenBy(c => c.IdCategoria)
            .Select(c => new RespuestaCategoriaDTO
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.nombreCategoria,
                Estado = c.Estado,
                UrlImagen = c.UrlImagen
            })
            .ToListAsync(cancellationToken);
    }
}
