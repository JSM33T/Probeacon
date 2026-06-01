using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using ProBeacon.Application.Common.Options;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Setup.Commands;

public class SetupCommandHandler(
    IApplicationDbContext db,
    ITenantProvisioner tenantProvisioner,
    IOptions<AppOptions> appOptions)
    : IRequestHandler<SetupCommand, SetupResult>
{
    public async ValueTask<SetupResult> Handle(SetupCommand request, CancellationToken cancellationToken)
    {
        if (!appOptions.Value.IsSelfHosted)
            throw new ForbiddenException("Self-hosted setup is disabled in online demo mode.");

        if (await db.Tenants.AnyAsync(cancellationToken))
            throw new ConflictException("ProBeacon is already configured.");

        var result = await tenantProvisioner.ProvisionAsync(
            new TenantProvisioningRequest(
                request.OrgName,
                request.AdminName,
                request.Email,
                request.Password,
                TenantKind.SelfHosted,
                null),
            cancellationToken);

        return new SetupResult(
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
            result.Role
        );
    }
}
