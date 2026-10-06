using API_SISTEMA.data;
using API_SISTEMA.DTOs.Marcas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Marca;

public class MarcaListarService
{
    private readonly SistemaDbContext _context;

    public MarcaListarService(SistemaDbContext context)
    {
        _context = context;
    }

    // Consulta de solo lectura: devuelve las marcas activas ordenadas por nombre e ID.
    public Task<List<RespuestaMarcaDTO>> ListarAdministracion(CancellationToken cancellationToken = default)
        => _context.Marcas.AsNoTracking()
            .OrderBy(m => m.Nombre).ThenBy(m => m.IdMarca)
            .Select(m => new RespuestaMarcaDTO
            {
                IdMarca = m.IdMarca, Nombre = m.Nombre, IdCategoria = m.IdCategoria,
                NombreCategoria = m.Categoria.nombreCategoria, Estado = m.Estado, UrlImagen = m.UrlImagen
            }).ToListAsync(cancellationToken);

    public Task<List<RespuestaMarcaDTO>> ListarActivas(CancellationToken cancellationToken = default)
    {
        return _context.Marcas.AsNoTracking()
            .Where(c => c.Estado)
            .OrderBy(c => c.Nombre).ThenBy(c => c.IdMarca)
            .Select(c => new RespuestaMarcaDTO
            {
                IdMarca =c.IdMarca,
                IdCategoria = c.IdCategoria,
                // Obtiene el nombre de la categoría relacionada, no el de la marca.
                NombreCategoria = c.Categoria.nombreCategoria,
                Nombre = c.Nombre,
                UrlImagen = c.UrlImagen,
                Estado = c.Estado
            })
            .ToListAsync(cancellationToken);
    }
}
