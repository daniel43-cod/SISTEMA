using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public sealed class EditarReceptorVentaService(SistemaDbContext db, PermisoUsuarioService permisos)
{
    public async Task<DatosReceptorVenta> Editar(int idVenta, EditarReceptorVentaDto dto, int usuario, CancellationToken ct)
    {
        if (!await permisos.TienePermiso(usuario, Utilidades.Permisos.Vender)) throw new UnauthorizedAccessException("No tienes permiso para editar el receptor.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await AccesoVenta.Validar(db, idVenta, usuario, ct);
        var venta = await db.ventas.FromSqlInterpolated($"SELECT * FROM public.ventas WHERE id_ventas = {idVenta} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("La venta no existe.");
        if (await db.DocumentosFel.AnyAsync(d => d.id_venta == idVenta && d.estado != "RECHAZADA", ct))
            throw new VentaConflictoException("No puedes cambiar el receptor de una venta con una factura certificada o pendiente de confirmar.");
        var receptor = ReceptorVentaReglas.Validar(dto.tipo_identificacion_receptor, dto.identificacion_receptor, dto.nombre_receptor, venta.total);
        venta.tipo_identificacion_receptor = receptor.Tipo;
        venta.identificacion_receptor = receptor.Identificacion;
        venta.nombre_receptor = receptor.Nombre;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return receptor;
    }
}
