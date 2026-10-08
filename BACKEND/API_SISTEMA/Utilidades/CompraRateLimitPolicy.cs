using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Utilidades;

public sealed class CompraRateLimitPolicy : IRateLimiterPolicy<string>
{
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
            ? Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) : 60;
        context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            mensaje = "Demasiadas solicitudes de compras. Espera antes de reintentar."
        }, ct);
    };

    public RateLimitPartition<string> GetPartition(HttpContext context) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60, Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0, AutoReplenishment = true
            });
}
