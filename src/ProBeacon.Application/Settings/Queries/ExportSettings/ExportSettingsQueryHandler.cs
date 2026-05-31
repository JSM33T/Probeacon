using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Settings.Queries.ExportSettings;

public class ExportSettingsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ExportSettingsQuery, SettingsExport>
{
    public async ValueTask<SettingsExport> Handle(ExportSettingsQuery request, CancellationToken cancellationToken)
    {
        var rows = await db.TenantSettings
            .Where(s => s.TenantId == currentUser.TenantId)
            .OrderBy(s => s.Key)
            .Select(s => new { s.Key, s.Value })
            .ToListAsync(cancellationToken);

        var settings = new Dictionary<string, string>();
        var redacted = new List<string>();

        foreach (var row in rows)
        {
            if (!request.IncludeSecrets && SettingsSecrets.IsSecretKey(row.Key))
            {
                settings[row.Key] = SettingsSecrets.RedactedValue;
                redacted.Add(row.Key);
            }
            else
            {
                settings[row.Key] = row.Value;
            }
        }

        return new SettingsExport
        {
            ExportedAt = DateTime.UtcNow,
            IncludesSecrets = request.IncludeSecrets,
            Settings = settings,
            RedactedKeys = redacted,
        };
    }
}
