using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Settings.Queries.GetSmtpSettings;

public class GetSmtpSettingsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetSmtpSettingsQuery, SmtpSettingsDto>
{
    public async ValueTask<SmtpSettingsDto> Handle(GetSmtpSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await db.TenantSettings
            .Where(s => s.TenantId == currentUser.TenantId && s.Key.StartsWith("smtp."))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var host = settings.GetValueOrDefault("smtp.host", "");
        var port = int.TryParse(settings.GetValueOrDefault("smtp.port", "587"), out var p) ? p : 587;
        var username = settings.GetValueOrDefault("smtp.username", "");
        var hasPassword = settings.ContainsKey("smtp.password") && !string.IsNullOrEmpty(settings["smtp.password"]);
        var fromAddress = settings.GetValueOrDefault("smtp.from_address", "");
        var fromName = settings.GetValueOrDefault("smtp.from_name", "ProBeacon");
        var enableSsl = settings.GetValueOrDefault("smtp.ssl", "true") == "true";
        var isConfigured = !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(fromAddress);

        return new SmtpSettingsDto(host, port, username, hasPassword, fromAddress, fromName, enableSsl, isConfigured);
    }
}
