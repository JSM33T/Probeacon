using System.Text.RegularExpressions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Settings.Commands.ImportSettings;

public partial class ImportSettingsCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ImportSettingsCommand, ImportSettingsResult>
{
    [GeneratedRegex(@"^[a-z][a-z0-9]*(\.[a-z0-9][a-z0-9_-]*)*$")]
    private static partial Regex KeyRegex();

    public async ValueTask<ImportSettingsResult> Handle(ImportSettingsCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportSettingsResult();

        var existing = await db.TenantSettings
            .Where(s => s.TenantId == currentUser.TenantId)
            .ToDictionaryAsync(s => s.Key, s => s, cancellationToken);

        var seen = new HashSet<string>();

        foreach (var (rawKey, rawValue) in request.Settings)
        {
            var key = rawKey.Trim();
            seen.Add(key);

            if (!KeyRegex().IsMatch(key))
            {
                result.Skipped.Add(new SkippedSetting(key, "Invalid key format — use lowercase dotted keys."));
                continue;
            }

            var value = rawValue ?? string.Empty;

            // Redacted secret placeholder — leave the stored value untouched.
            if (value == SettingsSecrets.RedactedValue)
            {
                if (existing.ContainsKey(key))
                    result.SecretsPreserved++;
                else
                    result.Skipped.Add(new SkippedSetting(key, "Redacted placeholder with no existing value to preserve."));
                continue;
            }

            try
            {
                if (existing.TryGetValue(key, out var setting))
                {
                    setting.UpdateValue(value);
                    result.Updated++;
                }
                else
                {
                    db.TenantSettings.Add(TenantSetting.Create(currentUser.TenantId, key, value));
                    result.Created++;
                }
            }
            catch (ArgumentException ex)
            {
                // Domain validation (ValidationRegex) rejected the value.
                result.Skipped.Add(new SkippedSetting(key, ex.Message));
            }
        }

        if (request.Replace)
        {
            foreach (var (key, setting) in existing)
            {
                if (!seen.Contains(key))
                {
                    db.TenantSettings.Remove(setting);
                    result.Deleted++;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return result;
    }
}
