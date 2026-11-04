using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public sealed class ReporteVentasIntervaloService(SistemaDbContext context, TimeProvider reloj)
{
    public async Task<ReporteVentasAcumuladasDto> ConsultarAcumulado(int idUsuario, int idSesionCaja, CancellationToken ct = default)
    {
        if (idSesionCaja <= 0) throw new ValidationException("La sesión de caja no es válida.");
        if (!await context.usuarios.AsNoTracking().AnyAsync(u => u.id_usuario == idUsuario && u.estado &&
            u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede consultar este reporte.");
        var sesion = await context.sesioncaja.AsNoTracking().SingleOrDefaultAsync(s => s.id_sesion_caja == idSesionCaja, ct)
            ?? throw new KeyNotFoundException("La sesión de caja no existe.");
        var ahora = TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala")).DateTime;
        var hasta = sesion.fecha_cierre ?? ahora;
        if (hasta < sesion.fecha_apertura) throw new ValidationException("Las fechas de la sesión de caja no son válidas.");
        var ventas = await context.ventas.AsNoTracking().Where(v => v.id_sesion_caja == idSesionCaja &&
            v.fecha_venta >= sesion.fecha_apertura && v.fecha_venta <= hasta)
            .OrderBy(v => v.fecha_venta).ThenBy(v => v.id_ventas)
            .Select(v => new { v.id_ventas, v.fecha_venta, v.total, v.ganancia_total }).ToListAsync(ct);
        List<PuntoVentaAcumuladaDto> puntos = [new() { Fecha = sesion.fecha_apertura }];
        decimal acumulado = 0;
        foreach (var venta in ventas)
        {
            ct.ThrowIfCancellationRequested();
            acumulado += venta.total;
            puntos.Add(new() { IdVenta = venta.id_ventas, Fecha = venta.fecha_venta, MontoVenta = venta.total,
                GananciaVenta = venta.ganancia_total, Acumulado = acumulado });
        }
        return new() { IdSesionCaja = idSesionCaja, Apertura = sesion.fecha_apertura, Cierre = sesion.fecha_cierre,
            Hasta = hasta, TotalVentas = acumulado, CantidadVentas = ventas.Count, Puntos = puntos };
    }

    public async Task<ReporteVentasIntervaloDto> Consultar(int idUsuario, FiltroReporteVentasIntervaloDto filtro,
        CancellationToken ct = default)
    {
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        if (!await context.usuarios.AsNoTracking().AnyAsync(u => u.id_usuario == idUsuario && u.estado &&
            u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede consultar este reporte.");
        var sesion = await context.sesioncaja.AsNoTracking().SingleOrDefaultAsync(s => s.id_sesion_caja == filtro.IdSesionCaja, ct)
            ?? throw new KeyNotFoundException("La sesión de caja no existe.");
        // Las fechas de ventas y caja existentes se almacenan en hora local del negocio.
        var ahora = TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala")).DateTime;
        var hasta = sesion.fecha_cierre ?? ahora;
        if (hasta < sesion.fecha_apertura) throw new ValidationException("Las fechas de la sesión de caja no son válidas.");
        var paso = TimeSpan.FromMinutes(filtro.IntervaloMinutos);
        var cantidad = Math.Max(1L, (hasta.Ticks - sesion.fecha_apertura.Ticks + paso.Ticks - 1) / paso.Ticks);
        if (cantidad > 5000) throw new ValidationException("La sesión supera 5000 intervalos. Selecciona un intervalo de 60 minutos o una sesión más corta.");
        var puntos = Enumerable.Range(0, (int)cantidad).Select(i =>
        {
            var desde = sesion.fecha_apertura.AddTicks(i * paso.Ticks);
            return new PuntoVentasIntervaloDto { Desde = desde, Hasta = hasta - desde < paso ? hasta : desde.Add(paso) };
        }).ToList();
        // Proyección mínima y lectura progresiva: no carga entidades ni detalles de productos.
        var ventas = context.ventas.AsNoTracking().Where(v => v.id_sesion_caja == filtro.IdSesionCaja &&
            v.fecha_venta >= sesion.fecha_apertura && v.fecha_venta <= hasta)
            .Select(v => new { v.fecha_venta, v.total }).AsAsyncEnumerable();
        decimal total = 0;
        var numeroVentas = 0;
        await foreach (var venta in ventas.WithCancellation(ct))
        {
            var indice = (int)Math.Min((venta.fecha_venta.Ticks - sesion.fecha_apertura.Ticks) / paso.Ticks, cantidad - 1);
            puntos[indice].TotalVentas += venta.total;
            puntos[indice].CantidadVentas++;
            total += venta.total;
            numeroVentas++;
        }
        return new ReporteVentasIntervaloDto
        {
            IdSesionCaja = sesion.id_sesion_caja, Apertura = sesion.fecha_apertura, Cierre = sesion.fecha_cierre,
            Hasta = hasta, IntervaloMinutos = filtro.IntervaloMinutos, TotalVentas = total,
            CantidadVentas = numeroVentas, Puntos = puntos
        };
    }
}
