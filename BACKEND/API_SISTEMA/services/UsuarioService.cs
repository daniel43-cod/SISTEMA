using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services;

public class UsuarioService(SistemaDbContext context)
{
    private static readonly Expression<Func<Usuario, UsuarioRespuestaDto>> Respuesta = u => new()
    {
        id_usuario = u.id_usuario, id_rol = u.id_rol, nombre = u.nombre,
        apellido = u.apellido, usuario = u.usuario, correo = u.correo,
        telefono = u.telefono, estado = u.estado, fecha_Creacion = u.fecha_Creacion
    };

    public Task<List<UsuarioRespuestaDto>> ListarUsuario() =>
        context.usuarios.AsNoTracking().Select(Respuesta).ToListAsync();

    public async Task<UsuarioRespuestaDto> CrearUsuario(CrearCuentaDto dto)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true))
            throw new UsuarioValidationException(errors[0].ErrorMessage!);

        var username = dto.usuario.Trim();
        var phone = dto.telefono.Trim();
        var email = string.IsNullOrWhiteSpace(dto.corre_electronico) ? null : dto.corre_electronico.Trim();
        // Calcular el hash antes de tomar bloqueos en SQL Server.
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.password);

        // Protege comprobación y alta entre ambas rutas y múltiples instancias.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var role = await context.rols.AsNoTracking().FirstOrDefaultAsync(r => r.id_rol == dto.id_rol);
        if (role is null || !role.estado ||
            (role.nombre != Roles.Administrador && role.nombre != Roles.Vendedor))
            throw new UsuarioValidationException("Selecciona un rol interno activo y válido.");

        if (await context.usuarios.AnyAsync(u => u.usuario == username))
            throw new UsuarioValidationException("Ya existe un usuario con ese nombre de usuario.");
        if (email is not null && await context.usuarios.AnyAsync(u => u.correo == email))
            throw new UsuarioValidationException("Ya existe un usuario con ese correo electrónico.");
        if (await context.usuarios.AnyAsync(u => u.telefono == phone))
            throw new UsuarioValidationException("Ya existe un usuario con ese teléfono.");

        var user = new Usuario
        {
            nombre = dto.nombre.Trim(), apellido = dto.apellido.Trim(), usuario = username,
            correo = email, telefono = phone, password = hash, id_rol = role.id_rol,
            estado = true, fecha_Creacion = DateTime.Now
        };
        context.usuarios.Add(user);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Respuesta.Compile()(user);
    }
}