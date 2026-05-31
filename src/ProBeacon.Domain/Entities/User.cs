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

    public DateTime? EmailVerifiedAt { get; private set; }
    public string? EmailVerificationTokenHash { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    public bool IsEmailVerified => EmailVerifiedAt.HasValue;

    public void SetRole(UserRole role) => Role = role;

    public void Deactivate() => IsActive = false;

    public void UpdateDisplayName(string displayName) => DisplayName = displayName;

    public void UpdateEmail(string email)
    {
        Email = email.ToLowerInvariant();
        // email changed — require re-verification
        EmailVerifiedAt = null;
    }

    public void UpdatePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void SetVerificationToken(string tokenHash, DateTime expiresAt)
    {
        EmailVerificationTokenHash = tokenHash;
        EmailVerificationTokenExpiresAt = expiresAt;
    }

    public void MarkEmailVerified()
    {
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationTokenHash = null;
        EmailVerificationTokenExpiresAt = null;
    }
}
