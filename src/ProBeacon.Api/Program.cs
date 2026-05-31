using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProBeacon.Api.Middleware;
using ProBeacon.Api.Services;
using ProBeacon.Application;
using ProBeacon.Infrastructure;
using ProBeacon.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<SetupState>();
builder.Services.AddHttpContextAccessor();

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
            RoleClaimType = "role"
        };
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

app.UseHttpsRedirection();
app.UseMiddleware<SetupGuardMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
