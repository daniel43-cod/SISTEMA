using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Login;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services;

public class LoginService(SistemaDbContext context, JwtService jwtService,
    API_SISTEMA.services.Auditoria.AuditoriaService auditoria)
{
    // Evita omitir BCrypt cuando no existe la cuenta.
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    public async Task<LoginRespuestaDTOs?> Login(LoginDTOs dto, CancellationToken cancellationToken = default)
    {
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), [], true))
            return null;

        var username = dto.usuario.Trim();
        // No elegir una cuenta arbitrariamente ante duplicados preexistentes.
        var users = await context.usuarios.AsNoTracking().Include(u => u.rol)
            .Where(u => u.usuario == username).Take(2).ToListAsync(cancellationToken);
        var user = users.Count == 1 ? users[0] : null;
        var valid = BCrypt.Net.BCrypt.Verify(dto.password, user?.password ?? DummyHash);
        if (!valid || user is null || !user.estado || user.rol is null || !user.rol.estado ||
            (user.rol.nombre != Roles.Administrador && user.rol.nombre != Roles.Vendedor))
        {
            await auditoria.RegistrarAutenticacionFallida(username, cancellationToken);
            return null;
        }

        var idSesion = Guid.NewGuid();
        var token = jwtService.GenerarToken(user, idSesion);
        var sesion = new API_SISTEMA.models.SesionUsuario
        {
            IdSesion = idSesion, IdUsuario = user.id_usuario, FechaCreacion = DateTime.UtcNow,
            FechaVencimiento = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo,
            TokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token))
        };
        // No entregar el token si no fue posible persistir el evento de auditoría.
        await auditoria.RegistrarInicioSesion(user, sesion, cancellationToken);
        return new LoginRespuestaDTOs
        {
            id_usuario = user.id_usuario, nombre = user.nombre,
            rol = user.rol.nombre, token = token
        };
    }
}
