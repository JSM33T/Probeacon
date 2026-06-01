using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Options;
using ProBeacon.Infrastructure.Auth;
using ProBeacon.Infrastructure.Email;
using ProBeacon.Infrastructure.Messaging;
using ProBeacon.Infrastructure.Persistence;
using ProBeacon.Infrastructure.Tenants;

namespace ProBeacon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRequestContext, RequestContext>();

        services.Configure<AppOptions>(opts =>
        {
            var section = configuration.GetSection("App");
            opts.FrontendUrl = section["FrontendUrl"] ?? string.Empty;
            opts.DeploymentMode = Enum.TryParse<DeploymentMode>(section["DeploymentMode"], true, out var deploymentMode)
                ? deploymentMode
                : DeploymentMode.SelfHosted;
        });

        services.Configure<DemoOptions>(opts =>
        {
            var section = configuration.GetSection("Demo");
            opts.WorkspaceLifetimeHours = int.TryParse(section["WorkspaceLifetimeHours"], out var lifetimeHours)
                ? lifetimeHours
                : 48;
            opts.CleanupIntervalMinutes = int.TryParse(section["CleanupIntervalMinutes"], out var cleanupIntervalMinutes)
                ? cleanupIntervalMinutes
                : 60;
        });

        services.Configure<RabbitMqOptions>(opts =>
        {
            var section = configuration.GetSection("RabbitMq");
            opts.Host = section["Host"] ?? "localhost";
            opts.Port = int.TryParse(section["Port"], out var port) ? port : 5672;
            opts.Username = section["Username"] ?? "guest";
            opts.Password = section["Password"] ?? "guest";
            opts.VirtualHost = section["VirtualHost"] ?? "/";
            opts.EmailQueue = section["EmailQueue"] ?? "probeacon.email";
        });
        services.AddSingleton<IEmailJobPublisher, RabbitMqEmailPublisher>();
        services.AddHostedService<EmailConsumerService>();

        services.Configure<EmailOptions>(opts =>
        {
            var section = configuration.GetSection("Email");
            opts.Host = section["Host"] ?? string.Empty;
            opts.Port = int.TryParse(section["Port"], out var port) ? port : 587;
            opts.Username = section["Username"] ?? string.Empty;
            opts.Password = section["Password"] ?? string.Empty;
            opts.FromAddress = section["FromAddress"] ?? string.Empty;
            opts.FromName = section["FromName"] ?? "ProBeacon";
        });
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddHostedService<OnlineDemoCleanupService>();

        return services;
    }
}
