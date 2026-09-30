using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Login;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services;

public class LoginService(SistemaDbContext context, JwtService jwtService)
{
    // Evita omitir BCrypt cuando no existe la cuenta.
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    public async Task<LoginRespuestaDTOs?> Login(LoginDTOs dto)
    {
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), [], true))
            return null;

        var username = dto.usuario.Trim();
        // No elegir una cuenta arbitrariamente ante duplicados preexistentes.
        var users = await context.usuarios.AsNoTracking().Include(u => u.rol)
            .Where(u => u.usuario == username).Take(2).ToListAsync();
        var user = users.Count == 1 ? users[0] : null;
        var valid = BCrypt.Net.BCrypt.Verify(dto.password, user?.password ?? DummyHash);
        if (!valid || user is null || !user.estado || user.rol is null || !user.rol.estado ||
            (user.rol.nombre != Roles.Administrador && user.rol.nombre != Roles.Vendedor))
            return null;

        return new LoginRespuestaDTOs
        {
            id_usuario = user.id_usuario, nombre = user.nombre,
            rol = user.rol.nombre, token = jwtService.GenerarToken(user)
        };
    }
}