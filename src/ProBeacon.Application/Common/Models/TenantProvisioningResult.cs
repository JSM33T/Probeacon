using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Common.Models;

public record TenantProvisioningResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid SessionId,
    Guid TenantId,
    string TenantSlug,
    TenantKind TenantKind,
    DateTime? TenantExpiresAt,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role
);
