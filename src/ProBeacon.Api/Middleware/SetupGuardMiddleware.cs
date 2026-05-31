using Microsoft.EntityFrameworkCore;
using ProBeacon.Api.Services;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Api.Middleware;

public class SetupGuardMiddleware(RequestDelegate next, SetupState setupState)
{
    public async Task InvokeAsync(HttpContext context, IApplicationDbContext db)
    {
        // Cache check — only hit DB once
        if (!setupState.IsConfigured.HasValue)
        {
            setupState.IsConfigured = await db.Tenants.AnyAsync();
        }

        // If not configured, only allow /api/setup through
        if (setupState.IsConfigured == false
            && !context.Request.Path.StartsWithSegments("/api/setup", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                configured = false,
                message = "ProBeacon is not configured. Complete setup at POST /api/setup."
            });
            return;
        }

        await next(context);
    }
}
