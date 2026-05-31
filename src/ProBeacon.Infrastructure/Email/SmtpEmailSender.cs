using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Infrastructure.Email;

public class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    private readonly EmailOptions _opts = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_opts.IsConfigured)
        {
            logger.LogWarning("Email not configured — skipping send to {To} (subject: {Subject})", to, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_opts.FromName, _opts.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_opts.Host, _opts.Port, SecureSocketOptions.StartTls, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_opts.Username))
            await client.AuthenticateAsync(_opts.Username, _opts.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
