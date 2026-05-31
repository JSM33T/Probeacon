using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Setup.Commands;

public class SetupCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRequestContext requestContext)
    : IRequestHandler<SetupCommand, SetupResult>
{
    public async ValueTask<SetupResult> Handle(SetupCommand request, CancellationToken cancellationToken)
    {
        if (await db.Tenants.AnyAsync(cancellationToken))
            throw new InvalidOperationException("ProBeacon is already configured.");

        var tenant = Tenant.Create(request.OrgName);
        db.Tenants.Add(tenant);

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(tenant.Id, request.Email, request.AdminName, passwordHash);
        db.Users.Add(user);

        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var session = UserSession.Create(
            user.Id,
            tenant.Id,
            tokenService.HashRefreshToken(rawRefreshToken),
            requestContext.UserAgent,
            requestContext.IpAddress);
        db.UserSessions.Add(session);

        await db.SaveChangesAsync(cancellationToken);

        var token = tokenService.GenerateAccessToken(user, tenant.Name, session.Id);

        return new SetupResult(
            token.AccessToken,
            token.ExpiresAt,
            rawRefreshToken,
            session.Id,
            user.Id,
            user.Email,
            user.DisplayName
        );
    }
}
