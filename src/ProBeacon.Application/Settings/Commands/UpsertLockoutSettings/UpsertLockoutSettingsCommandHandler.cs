using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using ProBeacon.Application.Settings.Queries.GetLockoutSettings;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Settings.Commands.UpsertLockoutSettings;

public class UpsertLockoutSettingsCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    ILockoutPolicyProvider provider)
    : IRequestHandler<UpsertLockoutSettingsCommand, LockoutSettingsDto>
{
    // Defence-in-depth alongside the FluentValidation rules — the regex guards the stored value.
    private static readonly Dictionary<string, string?> Regexes = new()
    {
        [LockoutSettingKeys.Enabled]     = @"^(true|false)$",
        [LockoutSettingKeys.MaxAttempts] = @"^\d{1,3}$",
        [LockoutSettingKeys.BaseMinutes] = @"^\d{1,4}$",
        [LockoutSettingKeys.MaxMinutes]  = @"^\d{1,4}$",
    };

    public async ValueTask<LockoutSettingsDto> Handle(UpsertLockoutSettingsCommand request, CancellationToken cancellationToken)
    {
        var toUpsert = new Dictionary<string, string>
        {
            [LockoutSettingKeys.Enabled]     = request.Enabled ? "true" : "false",
            [LockoutSettingKeys.MaxAttempts] = request.MaxAttempts.ToString(),
            [LockoutSettingKeys.BaseMinutes] = request.BaseMinutes.ToString(),
            [LockoutSettingKeys.MaxMinutes]  = request.MaxMinutes.ToString(),
        };

        var existing = await db.TenantSettings
            .Where(s => s.TenantId == currentUser.TenantId && s.Key.StartsWith(LockoutSettingKeys.Prefix))
            .ToDictionaryAsync(s => s.Key, s => s, cancellationToken);

        foreach (var (key, value) in toUpsert)
        {
            var regex = Regexes.GetValueOrDefault(key);

            if (existing.TryGetValue(key, out var setting))
            {
                setting.SetValidationRegex(regex);
                setting.UpdateValue(value);
            }
            else
            {
                db.TenantSettings.Add(TenantSetting.Create(currentUser.TenantId, key, value, regex));
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        // Return the canonical policy exactly as it will be read at login time.
        var policy = await provider.GetAsync(currentUser.TenantId, cancellationToken);
        return policy.ToDto();
    }
}
