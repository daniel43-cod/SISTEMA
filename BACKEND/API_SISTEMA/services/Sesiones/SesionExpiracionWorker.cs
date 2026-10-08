namespace API_SISTEMA.Services.Sesiones;

public sealed class SesionExpiracionWorker(IServiceScopeFactory scopes, ILogger<SesionExpiracionWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<SesionCierreService>().ProcesarVencidas(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "No se pudieron auditar las sesiones vencidas; se reintentará."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
