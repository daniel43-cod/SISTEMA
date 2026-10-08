using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Presentaciones;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Presentaciones;

public class ListarPresentacionService(SistemaDbContext context)
{
    public Task<List<PresentacionRespuestaDto>> ListarAdministracion(CancellationToken cancellationToken = default) =>
        context.presentaciones.AsNoTracking().OrderBy(p => p.Descripcion).ThenBy(p => p.IdPresentacion)
            .Select(p => new PresentacionRespuestaDto
            {
                IdPresentacion = p.IdPresentacion, Descripcion = p.Descripcion, Estado = p.Estado == true
            }).ToListAsync(cancellationToken);

    public Task<List<PresentacionRespuestaDto>> ListarActivas(CancellationToken cancellationToken = default) =>
        context.presentaciones.AsNoTracking()
            .Where(p => p.Estado == true)
            .OrderBy(p => p.Descripcion).ThenBy(p => p.IdPresentacion)
            .Select(p => new PresentacionRespuestaDto
            {
                IdPresentacion = p.IdPresentacion,
                Descripcion = p.Descripcion,
                Estado = true
            }).ToListAsync(cancellationToken);
}
