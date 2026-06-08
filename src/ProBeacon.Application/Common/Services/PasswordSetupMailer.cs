using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using ProBeacon.Application.Common.Options;
using ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Common.Services;

public class PasswordSetupMailer(
    IApplicationDbContext db,
    IEmailJobPublisher publisher,
    IRequestContext requestContext,
    IOptions<AppOptions> appOptions)
    : IPasswordSetupMailer
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan ResetLifetime = TimeSpan.FromHours(1);

    public async Task IssueAndSendAsync(User user, PasswordSetupKind kind, CancellationToken cancellationToken = default)
    {
        var (rawToken, tokenHash) = SecureToken.Generate();
        var lifetime = kind == PasswordSetupKind.Invite ? InviteLifetime : ResetLifetime;
        user.SetPasswordResetToken(tokenHash, DateTime.UtcNow.Add(lifetime));
        await db.SaveChangesAsync(cancellationToken);

        var baseUrl = string.IsNullOrWhiteSpace(appOptions.Value.FrontendUrl)
            ? requestContext.BaseUrl
            : appOptions.Value.FrontendUrl.TrimEnd('/');
        var link = $"{baseUrl}/set-password?token={rawToken}";

        var (subject, html) = kind == PasswordSetupKind.Invite
            ? ("Set up your ProBeacon account", BuildInviteHtml(user.DisplayName, link))
            : ("Reset your ProBeacon password", BuildResetHtml(user.DisplayName, link));

        await publisher.PublishAsync(new EmailJob(user.TenantId, user.Email, subject, html), cancellationToken);
    }

    private static string BuildInviteHtml(string name, string link) => BuildHtml(
        name,
        "You've been added to ProBeacon. Set a password to activate your account and sign in.",
        "Set your password",
        link,
        "This link expires in 7 days.");

    private static string BuildResetHtml(string name, string link) => BuildHtml(
        name,
        "We received a request to reset your ProBeacon password. Click below to choose a new one.",
        "Reset password",
        link,
        "This link expires in 1 hour. If you didn't request this, you can safely ignore this email.");

    private static string BuildHtml(string name, string intro, string cta, string link, string footer) => $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="font-family:sans-serif;color:#111;max-width:520px;margin:40px auto;padding:0 20px;">
          <h2 style="font-size:18px;margin-bottom:8px;">Hi {name},</h2>
          <p style="margin-bottom:24px;">{intro}</p>
          <a href="{link}"
             style="display:inline-block;padding:10px 20px;background:#0f172a;color:#fff;border-radius:6px;text-decoration:none;font-size:14px;">
            {cta}
          </a>
          <p style="margin-top:32px;font-size:12px;color:#888;">{footer}</p>
        </body>
        </html>
        """;
}
