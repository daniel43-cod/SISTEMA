using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public sealed class FiltrarVentasUsuarioService(SistemaDbContext context, VentaService consulta)
{
    public async Task<List<ListarVentasDto>> Listar(int idUsuarioAutenticado, int idUsuario,
        FiltrarVentasDto? filtro = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (idUsuario <= 0) throw new ValidationException("El identificador del usuario debe ser mayor a cero.");
        var solicitante = await context.usuarios.AsNoTracking().Include(u => u.rol)
            .SingleOrDefaultAsync(u => u.id_usuario == idUsuarioAutenticado, ct);
        if (solicitante is null || !solicitante.estado || solicitante.rol is null || !solicitante.rol.estado ||
            (solicitante.rol.nombre != Roles.Administrador && solicitante.rol.nombre != Roles.Vendedor))
            throw new UnauthorizedAccessException("No tienes permiso para consultar estas ventas.");
        if (solicitante.rol.nombre != Roles.Administrador && idUsuario != idUsuarioAutenticado)
            throw new UnauthorizedAccessException("Solo un administrador puede filtrar las ventas de otro usuario.");
        // Permite consultar ventas de responsables desactivados sin ocultar operaciones del turno.
        if (!await context.usuarios.AsNoTracking().AnyAsync(u => u.id_usuario == idUsuario, ct))
            throw new KeyNotFoundException("El usuario consultado no existe.");
        return await consulta.ConsultarVentas(idUsuarioAutenticado, filtro, idUsuario, ct);
    }
}
