using API_SISTEMA.data;
using API_SISTEMA.DTOs.Presentaciones;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Prestacion;

public class ListarPresentacionServices(SistemaDbContext context)
{
    public Task<List<PresentacionRespuestaDTO>> ListarAdministracion(CancellationToken cancellationToken = default) =>
        context.presentaciones.AsNoTracking().OrderBy(p => p.Descripcion).ThenBy(p => p.IdPresentacion)
            .Select(p => new PresentacionRespuestaDTO
            {
                IdPresentacion = p.IdPresentacion, Descripcion = p.Descripcion, Estado = p.Estado == true
            }).ToListAsync(cancellationToken);

    public Task<List<PresentacionRespuestaDTO>> ListarActivas(CancellationToken cancellationToken = default) =>
        context.presentaciones.AsNoTracking()
            .Where(p => p.Estado == true)
            .OrderBy(p => p.Descripcion).ThenBy(p => p.IdPresentacion)
            .Select(p => new PresentacionRespuestaDTO
            {
                IdPresentacion = p.IdPresentacion,
                Descripcion = p.Descripcion,
                Estado = true
            }).ToListAsync(cancellationToken);
}
