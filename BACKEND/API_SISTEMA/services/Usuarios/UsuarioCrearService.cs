using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Utilidades;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Usuarios;

public sealed class UsuarioCrearService(SistemaDbContext context, CatalogoAuditoriaService auditoria)
{
    public async Task<UsuarioRespuestaDto> CrearUsuario(CrearCuentaDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true))
            throw new UsuarioValidationException(errors[0].ErrorMessage!);

        var username = dto.usuario.Trim();
        var phone = dto.telefono.Trim();
        var email = string.IsNullOrWhiteSpace(dto.corre_electronico) ? null : dto.corre_electronico.Trim();
        if (phone.Any(c => !char.IsAsciiDigit(c) && c != '+' && c != '-' && c != ' ' && c != '(' && c != ')') ||
            !phone.Any(char.IsAsciiDigit))
            throw new UsuarioValidationException("Ingresa un teléfono válido con números y, opcionalmente, prefijo y separadores.");
        if (username.Any(char.IsControl) || dto.nombre.Any(char.IsControl) || dto.apellido.Any(char.IsControl))
            throw new UsuarioValidationException("El nombre, apellido y usuario no pueden contener caracteres de control.");

        var role = await context.rols.AsNoTracking().SingleOrDefaultAsync(r => r.id_rol == dto.id_rol, cancellationToken);
        if (role is null || !role.estado || (role.nombre != Roles.Administrador && role.nombre != Roles.Vendedor))
            throw new UsuarioValidationException("Selecciona un rol interno activo y válido.");
        if (await context.usuarios.AnyAsync(u => u.usuario == username, cancellationToken))
            throw new UsuarioConflictException("Ya existe un usuario con ese nombre de usuario.");
        if (email is not null && await context.usuarios.AnyAsync(u => u.correo == email, cancellationToken))
            throw new UsuarioConflictException("Ya existe un usuario con ese correo electrónico.");
        if (await context.usuarios.AnyAsync(u => u.telefono == phone, cancellationToken))
            throw new UsuarioConflictException("Ya existe un usuario con ese teléfono.");

        cancellationToken.ThrowIfCancellationRequested();
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.password);
        cancellationToken.ThrowIfCancellationRequested();
        // Los índices únicos de SQL Server resuelven altas concurrentes sin bloqueos Serializable.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var user = new Usuario
        {
            nombre = dto.nombre.Trim(), apellido = dto.apellido.Trim(), usuario = username,
            telefono = phone, correo = email, password = hash, id_rol = role.id_rol,
            estado = true, fecha_Creacion = DateTime.Now
        };
        try
        {
            context.usuarios.Add(user);
            await context.SaveChangesAsync(cancellationToken);
            await auditoria.Agregar("USUARIO_CREADO", "usuario", user.id_usuario, null,
                new Dictionary<string, object?> { ["usuario"] = username, ["id_rol"] = role.id_rol, ["estado"] = true }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        {
            throw new UsuarioConflictException("Ya existe un usuario con ese usuario, correo o teléfono.");
        }
        return new UsuarioRespuestaDto
        {
            id_usuario = user.id_usuario, id_rol = user.id_rol, nombre = user.nombre,
            apellido = user.apellido, usuario = user.usuario, correo = user.correo,
            telefono = user.telefono, estado = user.estado, fecha_Creacion = user.fecha_Creacion
        };
    }
}
