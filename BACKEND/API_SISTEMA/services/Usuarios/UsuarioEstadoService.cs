using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Security;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Usuarios;

public sealed class UsuarioEstadoService(SistemaDbContext context, ContextoPeticion peticion, CatalogoAuditoriaService auditoria)
{
    public async Task<UsuarioRespuestaDto?> CambiarEstado(int id, UsuarioEstadoDto dto, CancellationToken ct = default)
    {
        if (id <= 0 || dto?.estado is null) throw new UsuarioValidationException("Indica un usuario válido y su estado.");
        var estado = dto.estado.Value;
        var responsable = peticion.IdUsuario ?? throw new UsuarioValidationException("Se requiere un administrador autenticado.");
        if (!estado && id == responsable) throw new UsuarioValidationException("No puedes desactivar tu propia cuenta.");
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await context.usuarios.AnyAsync(u => u.id_usuario == responsable && u.estado && u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UsuarioValidationException("Se requiere un administrador activo.");
        var user = await context.usuarios.SingleOrDefaultAsync(u => u.id_usuario == id, ct);
        if (user is null) return null;
        if (user.estado != estado)
        {
            await auditoria.Agregar(estado ? "USUARIO_ACTIVADO" : "USUARIO_DESACTIVADO", "usuario", id,
                new() { ["estado"] = user.estado }, new() { ["estado"] = estado }, ct);
            user.estado = estado;
            if (!estado)
            {
                // Revocación permanente: reactivar la cuenta no rehabilita tokens anteriores.
                var ahora = DateTime.UtcNow;
                await context.SesionesUsuario.Where(s => s.IdUsuario == id && s.FechaRevocacion == null)
                    .ExecuteUpdateAsync(set => set.SetProperty(s => s.FechaRevocacion, ahora), ct);
            }
            await context.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);
        return new UsuarioRespuestaDto
        {
            id_usuario = user.id_usuario, id_rol = user.id_rol, nombre = user.nombre,
            apellido = user.apellido, usuario = user.usuario, correo = user.correo,
            telefono = user.telefono, estado = user.estado, fecha_Creacion = user.fecha_Creacion
        };
    }
}
