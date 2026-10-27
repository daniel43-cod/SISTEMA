using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Utilidades;

public sealed class PedidoCatalogoRateLimitPolicy : IRateLimiterPolicy<string>
{
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new { mensaje = "Demasiados pedidos. Espera antes de reintentar." }, ct);
    };

    public RateLimitPartition<string> GetPartition(HttpContext context)
        => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true });
}
