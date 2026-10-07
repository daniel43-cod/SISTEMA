using API_SISTEMA.data;
using API_SISTEMA.models;
using API_SISTEMA.Securyti;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Sesiones;

public sealed class SesionCierreService(SistemaDbContext context, ContextoPeticion peticion)
{

    // esta clase se encarga de cerra la sesion ya sea por expiracion o cierre voluntario
    public async Task<bool> Cerrar(Guid id, int usuarioId, bool expirada, CancellationToken ct = default)
    {
        var sesion = await context.SesionesUsuario.AsNoTracking().Include(s => s.Usuario)
            .SingleOrDefaultAsync(s => s.IdSesion == id && s.IdUsuario == usuarioId, ct);
        if (sesion is null) return false;
        var ahora = DateTime.UtcNow;
        var fecha = expirada ? sesion.FechaVencimiento : ahora;
        await using var tx = await context.Database.BeginTransactionAsync(ct);
        // UPDATE condicional: una sola petición/instancia gana el cierre, incluso con varios workers.
        var cambios = await context.SesionesUsuario.Where(s => s.IdSesion == id && s.IdUsuario == usuarioId &&
            s.FechaRevocacion == null && s.MotivoCierre == null &&
            (expirada ? s.FechaVencimiento <= ahora : s.FechaVencimiento > ahora))
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.FechaRevocacion, fecha)
                .SetProperty(s => s.MotivoCierre, expirada ? "EXPIRACION" : "VOLUNTARIO"), ct);
        if (cambios == 0) return false;
        var evento = new AuditoriaEvento
        {
            FechaUtc = fecha, IdUsuario = sesion.IdUsuario,
            UsuarioResponsable = sesion.Usuario.usuario[..Math.Min(sesion.Usuario.usuario.Length, 100)],
            Accion = expirada ? "SESION_EXPIRADA" : "SESION_CERRADA",
            Entidad = "sesiones_usuario", IdRegistro = id.ToString("D"), Resultado = "EXITOSO",
            Origen = expirada ? "SISTEMA" : "API",
            Motivo = expirada ? "Vencimiento del token." : "Cierre voluntario de sesión.",
            TraceId = expirada ? null : peticion.TraceId, DireccionIp = expirada ? null : peticion.DireccionIp
        };
        context.AuditoriaEventos.Add(evento);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        context.Entry(evento).State = EntityState.Detached;
        return true;
    }

    public async Task<int> ProcesarVencidas(CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        var vencidas = await context.SesionesUsuario.AsNoTracking()
            .Where(s => s.FechaRevocacion == null && s.MotivoCierre == null && s.FechaVencimiento <= ahora)
            .OrderBy(s => s.FechaVencimiento).Take(100)
            .Select(s => new { s.IdSesion, s.IdUsuario }).ToListAsync(ct);
        var total = 0;
        foreach (var s in vencidas)
            if (await Cerrar(s.IdSesion, s.IdUsuario, true, ct)) total++;
        return total;
    }
}
