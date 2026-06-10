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
    IRequestContext requestContext,
    ILockoutPolicyProvider lockoutPolicyProvider)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async ValueTask<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var user = await db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        // No such active account: generic failure (mirrors the original short-circuit — no extra
        // signal leaked) and nothing to lock.
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Lockout policy is configured per-workspace and read live (no app restart needed).
        var lockout = await lockoutPolicyProvider.GetAsync(user.TenantId, cancellationToken);

        // Block a locked account before checking the password, independent of whether the supplied
        // password is correct, so the lock can't be probed.
        if (lockout.Enabled && user.IsLockedOut(now))
        {
            var retryAfter = user.LockoutEndAt!.Value - now;
            throw new TooManyRequestsException(
                $"Too many failed sign-in attempts. Try again in {Math.Ceiling(retryAfter.TotalMinutes)} minute(s).",
                retryAfter);
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            if (lockout.Enabled)
            {
                user.RegisterFailedLogin(now, lockout.MaxAttempts, lockout.BaseLockout, lockout.MaxLockout);
                await db.SaveChangesAsync(cancellationToken);
            }

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Tenant.IsExpired(now))
            throw new WorkspaceExpiredException();

        user.RegisterSuccessfulLogin();

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
