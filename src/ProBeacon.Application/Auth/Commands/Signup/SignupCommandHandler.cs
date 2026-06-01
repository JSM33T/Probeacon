using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using ProBeacon.Application.Common.Options;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Auth.Commands.Signup;

public class SignupCommandHandler(
    IApplicationDbContext db,
    ITenantProvisioner tenantProvisioner,
    IOptions<AppOptions> appOptions,
    IOptions<DemoOptions> demoOptions)
    : IRequestHandler<SignupCommand, SignupResult>
{
    public async ValueTask<SignupResult> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        if (!appOptions.Value.IsOnlineDemo)
            throw new ForbiddenException("Online demo signup is disabled in self-hosted mode.");

        var now = DateTime.UtcNow;
        var email = request.Email.ToLowerInvariant();
        var existingUser = await db.Users
            .Include(user => user.Tenant)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.Tenant.IsExpired(now))
            {
                db.Tenants.Remove(existingUser.Tenant);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new ConflictException("An active demo workspace already exists for this email.");
            }
        }

        var expiresAt = now.AddHours(Math.Max(1, demoOptions.Value.WorkspaceLifetimeHours));
        var result = await tenantProvisioner.ProvisionAsync(
            new TenantProvisioningRequest(
                request.OrgName,
                request.AdminName,
                request.Email,
                request.Password,
                TenantKind.OnlineDemo,
                expiresAt),
            cancellationToken);

        return new SignupResult(
            result.AccessToken,
            result.AccessTokenExpiresAt,
            result.RefreshToken,
            result.SessionId,
            result.TenantId,
            result.TenantSlug,
            result.TenantKind.ToString(),
            result.TenantExpiresAt,
            result.UserId,
            result.Email,
            result.DisplayName,
            result.Role);
    }
}
