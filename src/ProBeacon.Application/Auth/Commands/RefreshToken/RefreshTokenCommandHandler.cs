using MediatR;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, RefreshResult>
{
    public async Task<RefreshResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);

        var session = await db.UserSessions
            .Include(s => s.User)
            .ThenInclude(u => u.Tenant)
            .FirstOrDefaultAsync(
                s => s.Id == request.SessionId && s.RefreshTokenHash == tokenHash,
                cancellationToken);

        if (session is null || session.IsRevoked || session.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        if (!session.User.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        var newRawToken = tokenService.GenerateRefreshToken();
        var newHash = tokenService.HashRefreshToken(newRawToken);
        session.RotateRefreshToken(newHash);

        var accessToken = tokenService.GenerateAccessToken(session.User, session.User.Tenant.Name, session.Id);
        await db.SaveChangesAsync(cancellationToken);

        return new RefreshResult(accessToken.AccessToken, accessToken.ExpiresAt, newRawToken);
    }
}
