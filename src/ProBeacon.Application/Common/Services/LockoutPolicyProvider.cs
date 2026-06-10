using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;

namespace ProBeacon.Application.Common.Services;

/// <summary>
/// Reads the per-tenant account-lockout policy live from <c>TenantSettings</c> (no app restart
/// needed). Missing keys fall back to <see cref="LockoutPolicy.Default"/>; stored values are
/// clamped to <see cref="LockoutLimits"/> so a bad/legacy value can never break login.
/// </summary>
public class LockoutPolicyProvider(IApplicationDbContext db) : ILockoutPolicyProvider
{
    public async Task<LockoutPolicy> GetAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantSettings
            .Where(s => s.TenantId == tenantId && s.Key.StartsWith(LockoutSettingKeys.Prefix))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var defaults = LockoutPolicy.Default;

        var enabled = ParseBool(settings.GetValueOrDefault(LockoutSettingKeys.Enabled), defaults.Enabled);

        var maxAttempts = Clamp(
            ParseInt(settings.GetValueOrDefault(LockoutSettingKeys.MaxAttempts), defaults.MaxAttempts),
            LockoutLimits.MinAttempts, LockoutLimits.MaxAttempts);

        var baseMinutes = Clamp(
            ParseInt(settings.GetValueOrDefault(LockoutSettingKeys.BaseMinutes), (int)defaults.BaseLockout.TotalMinutes),
            LockoutLimits.MinMinutes, LockoutLimits.MaxMinutes);

        var maxMinutes = Clamp(
            ParseInt(settings.GetValueOrDefault(LockoutSettingKeys.MaxMinutes), (int)defaults.MaxLockout.TotalMinutes),
            baseMinutes, LockoutLimits.MaxMinutes); // the ceiling can never sit below the base

        return new LockoutPolicy(
            enabled,
            maxAttempts,
            TimeSpan.FromMinutes(baseMinutes),
            TimeSpan.FromMinutes(maxMinutes));
    }

    private static bool ParseBool(string? value, bool fallback) =>
        bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;

    private static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
}
