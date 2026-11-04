using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Compras;

public sealed class ReporteComprasService(
    SistemaDbContext context,
    TimeProvider reloj)
{
    public async Task<ReporteComprasDto?> ConsultarCajaActiva(
        int idUsuario,
        CancellationToken ct = default)
    {
        // Verifica que el responsable sea un administrador activo.
        var autorizado = await context.usuarios
            .AsNoTracking()
            .AnyAsync(u =>
                u.id_usuario == idUsuario &&
                u.estado &&
                u.rol.estado &&
                u.rol.nombre == Roles.Administrador,
                ct);

        if (!autorizado)
        {
            throw new UnauthorizedAccessException(
                "Solo un administrador puede consultar este reporte.");
        }

        // Reutiliza la consulta existente del turno compartido.
        var sesion = await CajaSesionActual.Consultar(context, ct);

        if (sesion is null)
        {
            return null;
        }

        var hasta = TimeZoneInfo.ConvertTime(
            reloj.GetUtcNow(),
            TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala")
        ).DateTime;

        // Obtiene el total original de cada compra de esta sesión.
        var puntos = await context.registroCompras
            .AsNoTracking()
            .Where(c =>
                c.IdSesionCaja == sesion.id_sesion_caja &&
                c.FechaIngreso >= sesion.fecha_apertura &&
                c.FechaIngreso <= hasta)
            .OrderBy(c => c.FechaIngreso)
            .ThenBy(c => c.IdCompra)
            .Select(c => new PuntoCompraDto
            {
                IdCompra = c.IdCompra,
                Fecha = c.FechaIngreso,
                MontoCompra = c.TotalCompra
            })
            .ToListAsync(ct);

        return new ReporteComprasDto
        {
            IdSesionCaja = sesion.id_sesion_caja,
            Apertura = sesion.fecha_apertura,
            Hasta = hasta,
            TotalCompras = puntos.Sum(p => p.MontoCompra),
            CantidadCompras = puntos.Count,
            Puntos = puntos
        };
    }
}