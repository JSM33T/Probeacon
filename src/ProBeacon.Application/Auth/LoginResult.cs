namespace ProBeacon.Application.Auth;

public record LoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    string? RefreshToken,
    Guid SessionId,
    Guid TenantId,
    string TenantSlug,
    string TenantKind,
    DateTime? TenantExpiresAt,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role
);
