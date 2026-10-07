using System.Security.Claims;

namespace API_SISTEMA.Securyti;

// Información del servidor; nunca confiar en un usuario enviado por el cliente.
public sealed class ContextoPeticion(IHttpContextAccessor accessor)
{
    public Guid? IdSesion => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue("id_sesion"), out var id) ? id : null;
    public int? IdUsuario => int.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
        out var id) && id > 0 ? id : null;
    public string? TraceId => Limitar(accessor.HttpContext?.TraceIdentifier, 128);
    public string? DireccionIp => Limitar(accessor.HttpContext?.Connection.RemoteIpAddress?.ToString(), 45);

    private static string? Limitar(string? valor, int maximo)
        => valor is null ? null : valor[..Math.Min(valor.Length, maximo)];
}
