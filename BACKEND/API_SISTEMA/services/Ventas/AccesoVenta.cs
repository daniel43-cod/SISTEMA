using API_SISTEMA.Data;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Services.Ventas;

public static class AccesoVenta
{
    public static async Task<bool> EsAdministrador(SistemaDbContext db, int usuario, CancellationToken ct = default)
    {
        var rol = await db.usuarios.AsNoTracking().Where(u => u.id_usuario == usuario && u.estado && u.rol.estado)
            .Select(u => u.rol.nombre).SingleOrDefaultAsync(ct);
        if (rol is not (Roles.Administrador or Roles.Vendedor))
            throw new UnauthorizedAccessException("No tienes permiso para acceder a ventas.");
        return rol == Roles.Administrador;
    }

    public static async Task Validar(SistemaDbContext db, int venta, int usuario, CancellationToken ct = default)
    {
        if (venta <= 0) throw new ValidationException("El identificador de la venta debe ser mayor a cero.");
        var admin = await EsAdministrador(db, usuario, ct);
        var responsable = await db.ventas.AsNoTracking().Where(v => v.id_ventas == venta)
            .Select(v => (int?)v.id_usuario).SingleOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        if (!admin && responsable != usuario)
            throw new UnauthorizedAccessException("Solo puedes acceder a tus propias ventas.");
    }
}
