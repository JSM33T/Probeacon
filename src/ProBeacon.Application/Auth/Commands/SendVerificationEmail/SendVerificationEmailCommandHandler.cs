using System.Security.Cryptography;
using System.Text;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.SendVerificationEmail;

public class SendVerificationEmailCommandHandler(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IRequestContext requestContext,
    ILogger<SendVerificationEmailCommandHandler> logger)
    : ICommandHandler<SendVerificationEmailCommand>
{
    public async ValueTask<Unit> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null || user.IsEmailVerified)
            return default;

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)))
            .ToLowerInvariant();

        user.SetVerificationToken(tokenHash, DateTime.UtcNow.AddHours(24));
        await db.SaveChangesAsync(cancellationToken);

        var verificationUrl = $"{requestContext.BaseUrl}/verify-email?token={rawToken}";
        var html = BuildEmailHtml(user.DisplayName, verificationUrl);

        try
        {
            await emailSender.SendAsync(user.Email, "Verify your ProBeacon email", html, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send verification email to {Email}", user.Email);
        }

        return default;
    }

    private static string BuildEmailHtml(string name, string verificationUrl) => $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="font-family:sans-serif;color:#111;max-width:520px;margin:40px auto;padding:0 20px;">
          <h2 style="font-size:18px;margin-bottom:8px;">Verify your email address</h2>
          <p style="margin-bottom:24px;">Hi {name}, click the button below to verify your ProBeacon account.</p>
          <a href="{verificationUrl}"
             style="display:inline-block;padding:10px 20px;background:#0f172a;color:#fff;border-radius:6px;text-decoration:none;font-size:14px;">
            Verify email
          </a>
          <p style="margin-top:32px;font-size:12px;color:#888;">
            This link expires in 24 hours. If you didn't create a ProBeacon account, you can safely ignore this email.
          </p>
        </body>
        </html>
        """;
}
