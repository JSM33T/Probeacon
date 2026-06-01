using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Options;
using ProBeacon.Domain.Enums;
using ProBeacon.Infrastructure.Persistence;

namespace ProBeacon.Infrastructure.Tenants;

public class OnlineDemoCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<AppOptions> appOptions,
    IOptions<DemoOptions> demoOptions,
    ILogger<OnlineDemoCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!appOptions.Value.IsOnlineDemo)
            return;

        await CleanupOnceAsync(stoppingToken);

        var interval = TimeSpan.FromMinutes(Math.Max(1, demoOptions.Value.CleanupIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await CleanupOnceAsync(stoppingToken);
    }

    private async Task CleanupOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;

            var removed = await db.Tenants
                .Where(tenant =>
                    tenant.Kind == TenantKind.OnlineDemo
                    && tenant.ExpiresAt.HasValue
                    && tenant.ExpiresAt.Value <= now)
                .ExecuteDeleteAsync(cancellationToken);

            if (removed > 0)
                logger.LogInformation("Deleted {Count} expired online demo tenant(s).", removed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clean up expired online demo tenants.");
        }
    }
}
