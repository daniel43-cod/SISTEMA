using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Gastos;

public sealed class ReporteGastosService(
    SistemaDbContext context,
    TimeProvider reloj)
{
    public async Task<ReporteGastosDto?> ConsultarCajaActiva(
        int idUsuario,
        CancellationToken ct = default, int? idUsuarioFiltro = null)
    {
        // Comprueba que el usuario sea un administrador activo.
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

        if (idUsuarioFiltro.HasValue && idUsuarioFiltro.Value <= 0)
            throw new ValidationException("El ID del usuario debe ser mayor que cero.");

        var sesion = await CajaSesionActual.Consultar(context, ct);

        if (sesion is null)
        {
            return null;
        }

        var hasta = TimeZoneInfo.ConvertTime(
            reloj.GetUtcNow(),
            TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala")
        ).DateTime;

        // Recupera cada gasto de la sesión, sin acumular sus importes.
        var consulta = context.gastos
            .AsNoTracking()
            .Where(g =>
                g.id_sesion_caja == sesion.id_sesion_caja &&
                g.fecha >= sesion.fecha_apertura &&
                g.fecha <= hasta);

        if (idUsuarioFiltro.HasValue)
            consulta = consulta.Where(g => g.id_usuario == idUsuarioFiltro.Value);

        var puntos = await consulta
            .OrderBy(g => g.fecha)
            .ThenBy(g => g.id_gastos)
            .Select(g => new PuntoGastoDto
            {
                IdGasto = g.id_gastos,
                Fecha = g.fecha,
                MontoGasto = g.monto
            })
            .ToListAsync(ct);

        return new ReporteGastosDto
        {
            IdSesionCaja = sesion.id_sesion_caja,
            Apertura = sesion.fecha_apertura,
            Hasta = hasta,
            TotalGastos = puntos.Sum(p => p.MontoGasto),
            CantidadGastos = puntos.Count,
            Puntos = puntos
        };
    }
}