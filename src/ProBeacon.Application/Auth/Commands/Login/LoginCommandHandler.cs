using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRequestContext requestContext)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async ValueTask<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Tenant.IsExpired(DateTime.UtcNow))
            throw new WorkspaceExpiredException();

        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = tokenService.HashRefreshToken(rawRefreshToken);

        var session = UserSession.Create(
            user.Id,
            user.TenantId,
            refreshTokenHash,
            requestContext.UserAgent,
            requestContext.IpAddress);

        db.UserSessions.Add(session);

        var token = tokenService.GenerateAccessToken(user, user.Tenant, session.Id);
        await db.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            token.AccessToken,
            token.ExpiresAt,
            rawRefreshToken,
            session.Id,
            user.TenantId,
            user.Tenant.Slug,
            user.Tenant.Kind.ToString(),
            user.Tenant.ExpiresAt,
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString()
        );
    }
}
