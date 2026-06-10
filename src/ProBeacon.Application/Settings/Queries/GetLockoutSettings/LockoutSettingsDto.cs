using ProBeacon.Application.Common.Models;

namespace ProBeacon.Application.Settings.Queries.GetLockoutSettings;

public record LockoutSettingsDto(bool Enabled, int MaxAttempts, int BaseMinutes, int MaxMinutes);

internal static class LockoutSettingsMapping
{
    public static LockoutSettingsDto ToDto(this LockoutPolicy policy) =>
        new(
            policy.Enabled,
            policy.MaxAttempts,
            (int)policy.BaseLockout.TotalMinutes,
            (int)policy.MaxLockout.TotalMinutes);
}
