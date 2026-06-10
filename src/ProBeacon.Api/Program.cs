using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProBeacon.Api.Authorization;
using ProBeacon.Api.Middleware;
using ProBeacon.Api.RateLimiting;
using ProBeacon.Api.Services;
using ProBeacon.Application;
using ProBeacon.Infrastructure;
using ProBeacon.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton<SetupState>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProBeaconRateLimiting();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        var secret = builder.Configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        opts.MapInboundClaims = false; // keep claim names as-is (sub, tenant_id, etc.)
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            NameClaimType = "sub",
            RoleClaimType = "role",
            // Tighten the default 5-min leeway so a 15-min token doesn't effectively live ~20.
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole("Admin"));
});

var app = builder.Build();

// Migration-only mode — run pending migrations then exit
if (Environment.GetEnvironmentVariable("MIGRATE_ONLY") == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    return;
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler();
app.UseHttpsRedirection();

// Serve the built React SPA from wwwroot. Placed before the setup guard so the app shell
// and its assets always load (the client routes itself to /setup when needed).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseMiddleware<SetupGuardMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// SPA fallback: any route that isn't a real file and isn't under /api returns index.html,
// so React Router handles it client-side. Unknown /api routes stay a real 404.
var indexHtmlPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html");
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api") || !File.Exists(indexHtmlPath))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(indexHtmlPath);
});

app.Run();
