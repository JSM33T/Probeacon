using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Settings.Queries.GetSmtpSettings;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Settings.Commands.UpsertSmtpSettings;

public class UpsertSmtpSettingsCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpsertSmtpSettingsCommand, SmtpSettingsDto>
{
    // Validation regex per SMTP key — null means any value is accepted.
    private static readonly Dictionary<string, string?> Regexes = new()
    {
        ["smtp.host"]         = @"^[\w\-\.]+$",
        ["smtp.port"]         = @"^\d{1,5}$",
        ["smtp.username"]     = null,
        ["smtp.password"]     = null,
        ["smtp.from_address"] = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ["smtp.from_name"]    = null,
        ["smtp.ssl"]          = @"^(true|false)$",
    };

    public async ValueTask<SmtpSettingsDto> Handle(UpsertSmtpSettingsCommand request, CancellationToken cancellationToken)
    {
        var toUpsert = new Dictionary<string, string>
        {
            ["smtp.host"]         = request.Host.Trim(),
            ["smtp.port"]         = request.Port.ToString(),
            ["smtp.username"]     = request.Username.Trim(),
            ["smtp.from_address"] = request.FromAddress.Trim(),
            ["smtp.from_name"]    = request.FromName.Trim(),
            ["smtp.ssl"]          = request.EnableSsl ? "true" : "false",
        };

        if (!string.IsNullOrEmpty(request.Password))
            toUpsert["smtp.password"] = request.Password;

        var existing = await db.TenantSettings
            .Where(s => s.TenantId == currentUser.TenantId && s.Key.StartsWith("smtp."))
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

        var hasPassword = toUpsert.ContainsKey("smtp.password") ||
            (existing.ContainsKey("smtp.password") && !string.IsNullOrEmpty(existing["smtp.password"].Value));

        var isConfigured = !string.IsNullOrWhiteSpace(request.Host) && !string.IsNullOrWhiteSpace(request.FromAddress);

        return new SmtpSettingsDto(
            request.Host.Trim(),
            request.Port,
            request.Username.Trim(),
            hasPassword,
            request.FromAddress.Trim(),
            request.FromName.Trim(),
            request.EnableSsl,
            isConfigured);
    }
}
