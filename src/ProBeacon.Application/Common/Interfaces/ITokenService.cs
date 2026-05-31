using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Common.Interfaces;

public interface ITokenService
{
    TokenResult GenerateAccessToken(User user, string tenantName, Guid sessionId);
    string GenerateRefreshToken();
    string HashRefreshToken(string rawToken);
}

public record TokenResult(string AccessToken, DateTime ExpiresAt);
