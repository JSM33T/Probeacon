using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Infrastructure.Persistence;

namespace ProBeacon.Infrastructure.Email;

public class SmtpEmailSender(
    AppDbContext db,
    IOptions<EmailOptions> fallbackOptions,
    ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    public async Task SendAsync(Guid tenantId, string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var opts = await ResolveOptionsAsync(tenantId, cancellationToken);

        if (!opts.IsConfigured)
        {
            logger.LogWarning("SMTP not configured — skipping send to {To} (subject: {Subject})", to, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(opts.FromName, opts.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var sslMode = opts.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(opts.Host, opts.Port, sslMode, cancellationToken);

        if (!string.IsNullOrWhiteSpace(opts.Username))
            await client.AuthenticateAsync(opts.Username, opts.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task<EmailOptions> ResolveOptionsAsync(Guid tenantId, CancellationToken ct)
    {
        if (tenantId == Guid.Empty)
            return fallbackOptions.Value;

        var settings = await db.TenantSettings
            .Where(s => s.TenantId == tenantId && s.Key.StartsWith("smtp."))
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        if (!settings.ContainsKey("smtp.host") || string.IsNullOrWhiteSpace(settings["smtp.host"]))
            return fallbackOptions.Value;

        return new EmailOptions
        {
            Host = settings.GetValueOrDefault("smtp.host", ""),
            Port = int.TryParse(settings.GetValueOrDefault("smtp.port", "587"), out var p) ? p : 587,
            Username = settings.GetValueOrDefault("smtp.username", ""),
            Password = settings.GetValueOrDefault("smtp.password", ""),
            FromAddress = settings.GetValueOrDefault("smtp.from_address", ""),
            FromName = settings.GetValueOrDefault("smtp.from_name", "ProBeacon"),
            EnableSsl = settings.GetValueOrDefault("smtp.ssl", "true") == "true",
        };
    }
}
