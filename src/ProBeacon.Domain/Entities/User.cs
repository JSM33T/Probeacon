using ProBeacon.Domain.Enums;

namespace ProBeacon.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Tenant Tenant { get; private set; } = null!;

    private User() { }

    public static User Create(
        Guid tenantId,
        string email,
        string displayName,
        string passwordHash,
        UserRole role = UserRole.Admin) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Email = email.ToLowerInvariant(),
        DisplayName = displayName,
        PasswordHash = passwordHash,
        Role = role,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public void SetRole(UserRole role) => Role = role;

    public void Deactivate() => IsActive = false;
}
