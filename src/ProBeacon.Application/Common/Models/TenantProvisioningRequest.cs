using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Common.Models;

public record TenantProvisioningRequest(
    string OrganizationName,
    string AdminName,
    string Email,
    string Password,
    TenantKind TenantKind,
    DateTime? ExpiresAt
);
