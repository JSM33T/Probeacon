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

    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public bool IsEmailVerified => EmailVerifiedAt.HasValue;

    // ── Failed-login lockout ──────────────────────────────────────────────────────
    public int FailedLoginCount { get; private set; }
    public DateTime? LockoutEndAt { get; private set; }

    public bool IsLockedOut(DateTime utcNow) => LockoutEndAt is { } until && until > utcNow;

    /// <summary>
    /// Records a failed sign-in. Once <paramref name="threshold"/> consecutive failures are
    /// reached the account is locked, with the lockout doubling on each further failure (capped at
    /// <paramref name="maxLockout"/>) so a determined attacker faces escalating backoff.
    /// </summary>
    public void RegisterFailedLogin(DateTime utcNow, int threshold, TimeSpan baseLockout, TimeSpan maxLockout)
    {
        FailedLoginCount++;
        if (FailedLoginCount < threshold)
            return;

        var exponent = Math.Min(FailedLoginCount - threshold, 5); // cap doubling at 2^5
        var ticks = Math.Min(baseLockout.Ticks * (1L << exponent), maxLockout.Ticks);
        LockoutEndAt = utcNow.Add(TimeSpan.FromTicks(ticks));
    }

    /// <summary>Clears the failure counter and any active lock after a successful sign-in.</summary>
    public void RegisterSuccessfulLogin()
    {
        FailedLoginCount = 0;
        LockoutEndAt = null;
    }

    public void SetRole(UserRole role) => Role = role;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

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

    public void SetPasswordResetToken(string tokenHash, DateTime expiresAt)
    {
        PasswordResetTokenHash = tokenHash;
        PasswordResetTokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Applies a new password set via a reset/invite link, clears the token, and — since
    /// receiving the emailed link proves ownership of the address — marks the email verified.
    /// </summary>
    public void CompletePasswordReset(string passwordHash)
    {
        PasswordHash = passwordHash;
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAt = null;
        EmailVerifiedAt ??= DateTime.UtcNow;
    }
}
