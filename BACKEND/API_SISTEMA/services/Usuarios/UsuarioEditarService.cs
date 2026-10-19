using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Utilidades;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Usuarios;

public sealed class UsuarioEditarService(SistemaDbContext context, CatalogoAuditoriaService auditoria)
{
    public async Task<UsuarioRespuestaDto?> EditarUsuario(int id, EditarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new UsuarioValidationException("El ID del usuario debe ser mayor que cero.");
        ArgumentNullException.ThrowIfNull(dto);
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true))
            throw new UsuarioValidationException(errors[0].ErrorMessage!);
        var username = dto.usuario.Trim();
        var phone = dto.telefono.Trim();
        if (phone.Any(c => !char.IsAsciiDigit(c) && c != '+' && c != '-' && c != ' ' && c != '(' && c != ')') || !phone.Any(char.IsAsciiDigit))
            throw new UsuarioValidationException("Ingresa un teléfono válido con números y, opcionalmente, prefijo y separadores.");
        if (username.Any(char.IsControl) || dto.nombre.Any(char.IsControl) || dto.apellido.Any(char.IsControl))
            throw new UsuarioValidationException("El nombre, apellido y usuario no pueden contener caracteres de control.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var user = await context.usuarios.SingleOrDefaultAsync(u => u.id_usuario == id, cancellationToken);
        if (user is null) return null;
        var role = await context.rols.AsNoTracking().SingleOrDefaultAsync(r => r.id_rol == dto.id_rol, cancellationToken);
        if (role is null || !role.estado || (role.nombre != Roles.Administrador && role.nombre != Roles.Vendedor))
            throw new UsuarioValidationException("Selecciona un rol interno activo y válido.");
        if (await context.usuarios.AnyAsync(u => u.id_usuario != id && u.usuario == username, cancellationToken))
            throw new UsuarioConflictException("Ya existe otro usuario con ese nombre de usuario.");
        if (dto.corre_electronico is not null && await context.usuarios.AnyAsync(u => u.id_usuario != id && u.correo == dto.corre_electronico, cancellationToken))
            throw new UsuarioConflictException("Ya existe otro usuario con ese correo electrónico.");
        if (await context.usuarios.AnyAsync(u => u.id_usuario != id && u.telefono == phone, cancellationToken))
            throw new UsuarioConflictException("Ya existe otro usuario con ese teléfono.");

        var before = Campos(user);
        user.nombre = dto.nombre.Trim(); user.apellido = dto.apellido.Trim(); user.usuario = username;
        user.telefono = phone; user.correo = dto.corre_electronico; user.id_rol = role.id_rol;
        try
        {
            // Registrar antes de guardar conserva la identidad anterior del administrador si se edita a sí mismo.
            await auditoria.Agregar("USUARIO_EDITADO", "usuario", id, before, Campos(user), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        {
            throw new UsuarioConflictException("Ya existe otro usuario con ese usuario, correo o teléfono.");
        }
        return new UsuarioRespuestaDto
        {
            id_usuario = user.id_usuario, id_rol = user.id_rol, nombre = user.nombre,
            apellido = user.apellido, usuario = user.usuario, correo = user.correo,
            telefono = user.telefono, estado = user.estado, fecha_Creacion = user.fecha_Creacion
        };
    }

    private static Dictionary<string, object?> Campos(Usuario user) => new()
    {
        ["nombre"] = user.nombre, ["apellido"] = user.apellido, ["usuario"] = user.usuario,
        ["correo"] = user.correo, ["telefono"] = user.telefono, ["id_rol"] = user.id_rol
    };
}
