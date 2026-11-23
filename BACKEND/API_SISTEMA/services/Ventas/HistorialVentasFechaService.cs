using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public sealed class HistorialVentasFechaService(SistemaDbContext context, VentaService ventas, TimeProvider? reloj = null)
{
    public async Task<List<ListarVentasDto>> Listar(int idUsuario, HistorialVentasFechaDto filtro, CancellationToken ct = default)
        => (await ListarPagina(idUsuario, filtro, ct)).Items;

    public async Task<VentasResumenPaginaDto> ListarPagina(int idUsuario, HistorialVentasFechaDto filtro, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        if (!await context.usuarios.AsNoTracking().AnyAsync(u => u.id_usuario == idUsuario && u.estado &&
            u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede consultar el historial por fechas.");
        if (filtro.IdUsuario.HasValue && !await context.usuarios.AsNoTracking()
            .AnyAsync(u => u.id_usuario == filtro.IdUsuario.Value, ct))
            throw new KeyNotFoundException("El usuario consultado no existe.");
        if (filtro.IdCliente.HasValue)
            throw new ValidationException("El filtro de cliente está disponible únicamente para la caja activa.");
        var hoy = DateOnly.FromDateTime(RelojNegocio.Ahora(reloj));
        var hasta = filtro.Hasta ?? hoy;
        if (!filtro.Desde.HasValue && hasta.DayNumber < 6)
            throw new ValidationException("El rango de fechas no es válido.");
        var desde = filtro.Desde ?? hasta.AddDays(-6);
        if (desde > hasta || hasta == DateOnly.MaxValue)
            throw new ValidationException("El rango de fechas no es válido.");
        // fecha_venta almacena la hora local del negocio. No utilizar fecha de apertura/cierre.
        return await ventas.ConsultarPagina(idUsuario, new FiltrarVentasDto
        {
            pagina = filtro.Pagina, tamanoPagina = filtro.TamanoPagina, buscar = filtro.Buscar, estadoPago = filtro.EstadoPago,
            desde = desde.ToDateTime(TimeOnly.MinValue)
        }, filtro.IdUsuario, ct, soloCajaActiva: false, hastaExclusiva: hasta.AddDays(1).ToDateTime(TimeOnly.MinValue));
    }
}
