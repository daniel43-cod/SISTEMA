using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Services.Permisos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.Fel;

public sealed record EstadoVentaFelLista(int id_venta, string estado, string ambiente);

public sealed class ConsultarEstadosFelService(SistemaDbContext db, PermisoUsuarioService permisos, IOptions<InfileOptions> options)
{
    public async Task<List<EstadoVentaFelLista>> Consultar(int[]? ids, int usuario, CancellationToken ct)
    {
        if (ids is null || ids.Length is < 1 or > 100 || ids.Any(i => i < 1) || ids.Distinct().Count() != ids.Length)
            throw new ValidationException("Consulta entre 1 y 100 ventas distintas con identificadores válidos.");
        if (!await permisos.TienePermiso(usuario, Utilidades.Permisos.Vender))
            throw new UnauthorizedAccessException("No tienes permiso para consultar la certificación de ventas.");
        var ambiente = options.Value.Ambiente;
        // Proyección de metadatos en una sola consulta: no carga XML, intentos ni respuestas.
        var estados = await db.ventas.AsNoTracking().Where(v => ids.Contains(v.id_ventas)).OrderBy(v => v.id_ventas)
            .Select(v => new EstadoVentaFelLista(v.id_ventas,
                db.DocumentosFel.Where(d => d.id_venta == v.id_ventas && d.ambiente == ambiente)
                    .Select(d => d.estado).FirstOrDefault() ?? "SIN_CERTIFICAR", ambiente)).ToListAsync(ct);
        if (estados.Count != ids.Length) throw new KeyNotFoundException("Una de las ventas consultadas ya no existe.");
        return estados;
    }
}
