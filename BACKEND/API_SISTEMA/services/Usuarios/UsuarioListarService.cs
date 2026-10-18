using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Login;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Usuarios;

public sealed class UsuarioListarService(SistemaDbContext context)
{
    public Task<List<UsuarioRespuestaDto>> ListarUsuario(CancellationToken cancellationToken = default) =>
        context.usuarios.AsNoTracking()
            .OrderBy(u => u.nombre).ThenBy(u => u.apellido).ThenBy(u => u.id_usuario)
            .Select(u => new UsuarioRespuestaDto
            {
                id_usuario = u.id_usuario, id_rol = u.id_rol, nombre = u.nombre,
                apellido = u.apellido, usuario = u.usuario, correo = u.correo,
                telefono = u.telefono, estado = u.estado, fecha_Creacion = u.fecha_Creacion
            })
            .ToListAsync(cancellationToken);
}
