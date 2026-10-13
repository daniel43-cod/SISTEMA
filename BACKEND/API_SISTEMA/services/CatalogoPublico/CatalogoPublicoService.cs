using API_SISTEMA.Data;
using API_SISTEMA.Dtos.CatalogoPublico;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.CatalogoPublico;

public sealed class CatalogoPublicoService(SistemaDbContext context)
{
    // La visibilidad usa el estado actual hasta incorporar la administración pública.
    public Task<List<CategoriaPublicaDto>> ListarCategorias(CancellationToken cancellationToken = default)
        => context.categorias.AsNoTracking().Where(c => c.Estado)
            .OrderBy(c => c.nombreCategoria).ThenBy(c => c.IdCategoria).Select(c => new CategoriaPublicaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.nombreCategoria,
                UrlImagen = c.UrlImagen
            })
            .ToListAsync(cancellationToken);
}
