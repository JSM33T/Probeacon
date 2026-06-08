using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Services;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Auth.Commands.SetPassword;

public class SetPasswordCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRequestContext requestContext)
    : IRequestHandler<SetPasswordCommand, LoginResult>
{
    public async ValueTask<LoginResult> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = SecureToken.Hash(request.Token);

        var user = await db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(
                u => u.PasswordResetTokenHash == tokenHash
                    && u.PasswordResetTokenExpiresAt > DateTime.UtcNow
                    && u.IsActive,
                cancellationToken);

        if (user is null)
            throw new InvalidOperationException("This link is invalid or has expired.");

        if (user.Tenant.IsExpired(DateTime.UtcNow))
            throw new WorkspaceExpiredException();

        user.CompletePasswordReset(passwordHasher.Hash(request.Password));

        // Invalidate any existing sessions, then start a fresh one (auto-login).
        var sessions = await db.UserSessions
            .Where(session => session.UserId == user.Id && !session.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
            session.Revoke();

        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var newSession = UserSession.Create(
            user.Id,
            user.TenantId,
            tokenService.HashRefreshToken(rawRefreshToken),
            requestContext.UserAgent,
            requestContext.IpAddress);
        db.UserSessions.Add(newSession);

        var token = tokenService.GenerateAccessToken(user, user.Tenant, newSession.Id);
        await db.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            token.AccessToken,
            token.ExpiresAt,
            rawRefreshToken,
            newSession.Id,
            user.TenantId,
            user.Tenant.Slug,
            user.Tenant.Kind.ToString(),
            user.Tenant.ExpiresAt,
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString());
    }
}
