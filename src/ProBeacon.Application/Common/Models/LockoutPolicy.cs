namespace ProBeacon.Application.Common.Models;

/// <summary>
/// Effective per-workspace account-lockout policy, loaded from tenant settings with
/// <see cref="Default"/> applied for any unset key. For a self-hosted single-tenant install this is
/// effectively the instance-wide policy.
/// </summary>
public sealed record LockoutPolicy(bool Enabled, int MaxAttempts, TimeSpan BaseLockout, TimeSpan MaxLockout)
{
    public static readonly LockoutPolicy Default = new(
        Enabled: true,
        MaxAttempts: 5,
        BaseLockout: TimeSpan.FromMinutes(1),
        MaxLockout: TimeSpan.FromMinutes(30));
}

/// <summary>Tenant-setting keys backing the <see cref="LockoutPolicy"/>.</summary>
public static class LockoutSettingKeys
{
    public const string Prefix = "security.lockout.";
    public const string Enabled = Prefix + "enabled";
    public const string MaxAttempts = Prefix + "max_attempts";
    public const string BaseMinutes = Prefix + "base_minutes";
    public const string MaxMinutes = Prefix + "max_minutes";
}

/// <summary>Accepted bounds for lockout settings — enforced on write, clamped on read.</summary>
public static class LockoutLimits
{
    public const int MinAttempts = 1;
    public const int MaxAttempts = 100;
    public const int MinMinutes = 1;
    public const int MaxMinutes = 1440; // 24h
}
