using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Caja;

public static class CajaSesionActual
{
    // La caja pertenece al turno compartido, no al usuario que lo abrio.
    public static async Task<SesionCaja?> Consultar(SistemaDbContext context, CancellationToken ct = default)
    {
        var abiertas = await context.sesioncaja.AsNoTracking().Where(s => s.fecha_cierre == null).Take(2).ToListAsync(ct);
        if (abiertas.Count > 1) throw new CajaValidationException("Hay varias cajas abiertas. Un administrador debe revisar las sesiones.");
        return abiertas.SingleOrDefault();
    }

    public static async Task<SesionCaja> ParaOperacion(SistemaDbContext context, CancellationToken ct = default)
    {
        // El bloqueo de escritura se conserva hasta confirmar la operacion o el cierre.
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("La operacion de caja requiere una transaccion.");
        var sesion = await Consultar(context, ct) ?? throw new CajaValidationException("No hay una caja abierta.");
        var bloqueada = await context.sesioncaja.Where(s => s.id_sesion_caja == sesion.id_sesion_caja && s.fecha_cierre == null)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.monto_inicial, s => s.monto_inicial), ct);
        if (bloqueada != 1) throw new CajaValidationException("La caja fue cerrada. Actualiza antes de continuar.");
        return await context.sesioncaja.SingleAsync(s => s.id_sesion_caja == sesion.id_sesion_caja, ct);
    }
}
