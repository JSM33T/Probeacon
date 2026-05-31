namespace ProBeacon.Domain.Entities;

public class UserSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public User User { get; private set; } = null!;

    private UserSession() { }

    public static UserSession Create(
        Guid userId,
        Guid tenantId,
        string refreshTokenHash,
        string userAgent,
        string ipAddress,
        int refreshTokenDays = 30) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TenantId = tenantId,
        RefreshTokenHash = refreshTokenHash,
        UserAgent = userAgent,
        IpAddress = ipAddress,
        CreatedAt = DateTime.UtcNow,
        LastActiveAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
        IsRevoked = false
    };

    public void Revoke() => IsRevoked = true;

    public void UpdateLastActive() => LastActiveAt = DateTime.UtcNow;

    public void RotateRefreshToken(string newHash)
    {
        RefreshTokenHash = newHash;
        LastActiveAt = DateTime.UtcNow;
    }
}
