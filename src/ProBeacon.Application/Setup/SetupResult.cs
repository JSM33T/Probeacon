namespace ProBeacon.Application.Setup;

public record SetupResult(
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
