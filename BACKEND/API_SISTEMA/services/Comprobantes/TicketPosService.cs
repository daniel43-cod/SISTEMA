using API_SISTEMA.Data;
using API_SISTEMA.Services.Fel;
using API_SISTEMA.Services.Permisos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.Comprobantes;

public sealed class TicketPosService(SistemaDbContext db, PermisoUsuarioService permisos, IOptions<InfileOptions> options)
{
    public async Task<TicketPosDto> Obtener(int idVenta, int usuario, CancellationToken ct)
    {
        if (!await permisos.TienePermiso(usuario, Utilidades.Permisos.Vender)) throw new UnauthorizedAccessException("No tienes permiso para imprimir ventas.");
        var venta = await db.ventas.AsNoTracking().Include(v => v.usuario).Include(v => v.cliente)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.Producto)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.Surtido)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.producto_presentacion).ThenInclude(p => p.Presentacion)
            .SingleOrDefaultAsync(v => v.id_ventas == idVenta, ct) ?? throw new KeyNotFoundException("La venta no existe.");
        var documento = await db.DocumentosFel.AsNoTracking().SingleOrDefaultAsync(d => d.id_venta == idVenta && d.ambiente == options.Value.Ambiente, ct);
        if (documento?.estado == "CERTIFICADA") return TicketPos.DesdeCertificado(documento, (venta.usuario.nombre + " " + venta.usuario.apellido).Trim());
        var empresas = await db.empresa.AsNoTracking().Take(2).ToListAsync(ct);
        if (empresas.Count != 1) throw new System.ComponentModel.DataAnnotations.ValidationException("Debe existir exactamente una empresa configurada para imprimir.");
        return TicketPos.DesdeVenta(venta, empresas[0]);
    }
}
