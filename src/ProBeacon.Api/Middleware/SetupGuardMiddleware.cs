using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProBeacon.Api.Services;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Options;

namespace ProBeacon.Api.Middleware;

public class SetupGuardMiddleware(
    RequestDelegate next,
    SetupState setupState,
    IOptions<AppOptions> appOptions)
{
    public async Task InvokeAsync(HttpContext context, IApplicationDbContext db)
    {
        if (appOptions.Value.IsOnlineDemo)
        {
            await next(context);
            return;
        }

        // Only the API is gated by setup state. Static assets and SPA routes always load —
        // the client checks /api/setup/status and routes itself to /setup.
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Cache check - only hit DB once.
        if (!setupState.IsConfigured.HasValue)
        {
            setupState.IsConfigured = await db.Tenants.AnyAsync();
        }

        // If not configured, only allow setup endpoints through.
        if (setupState.IsConfigured == false
            && !context.Request.Path.StartsWithSegments("/api/setup", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                configured = false,
                deploymentMode = appOptions.Value.DeploymentMode.ToString(),
                message = "ProBeacon is not configured. Complete setup at POST /api/setup."
            });
            return;
        }

        await next(context);
    }
}
