using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public sealed class ResumenVentasService(SistemaDbContext context, VentaService ventas, HistorialVentasFechaService historial)
{
    public async Task<VentasResumenPaginaDto> Consultar(int idUsuario, HistorialVentasFechaDto filtro, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        if (!await context.usuarios.AsNoTracking().AnyAsync(u => u.id_usuario == idUsuario && u.estado &&
            u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede consultar ganancias.");
        if (filtro.Desde.HasValue || filtro.Hasta.HasValue) return await historial.ListarPagina(idUsuario, filtro, ct);
        if (filtro.IdUsuario.HasValue && !await context.usuarios.AnyAsync(u => u.id_usuario == filtro.IdUsuario, ct))
            throw new KeyNotFoundException("El usuario consultado no existe.");
        return await ventas.ConsultarPagina(idUsuario, new FiltrarVentasDto
        { pagina = filtro.Pagina, tamanoPagina = filtro.TamanoPagina }, filtro.IdUsuario, ct);
    }
}
