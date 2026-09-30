using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API_SISTEMA.data;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Utilidades;

public sealed class UsuarioTokenValidator(SistemaDbContext context, JwtService jwtService,
    ILogger<UsuarioTokenValidator> logger)
{
    public async Task ValidateAsync(TokenValidatedContext tokenContext)
    {
        var principal = tokenContext.Principal;
        var tipo = principal?.FindFirstValue("tipo_cuenta");
        // Mantener el flujo de clientes aislado: no interpretar sus IDs como usuarios internos.
        if (tipo == "cuenta_cliente")
        {
            var roles = principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            if (roles.Length != 1 || roles[0] != "CLIENTE")
                tokenContext.Fail("Sesión inválida.");
            return;
        }

        if (tipo != "usuario" || !int.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) || id <= 0)
        {
            tokenContext.Fail("Sesión inválida.");
            return;
        }

        try
        {
            var user = await context.usuarios.AsNoTracking().Include(u => u.rol)
                .SingleOrDefaultAsync(u => u.id_usuario == id, tokenContext.HttpContext.RequestAborted);
            var roles = principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            if (user is null || !user.estado || user.rol is null || !user.rol.estado ||
                (user.rol.nombre != Roles.Administrador && user.rol.nombre != Roles.Vendedor) ||
                roles.Length != 1 || roles[0] != user.rol.nombre ||
                principal.FindFirstValue("version_credencial") != jwtService.ObtenerVersion(user))
                tokenContext.Fail("Sesión inválida. Inicia sesión nuevamente.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "No se pudo validar la sesión de usuario.");
            tokenContext.Fail("No se pudo validar la sesión.");
        }
    }
}
