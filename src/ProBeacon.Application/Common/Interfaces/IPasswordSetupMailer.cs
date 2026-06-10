using ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Common.Interfaces;

public interface IPasswordSetupMailer
{
    /// <summary>
    /// Issues a single-use set-password token for the user, persists it, and returns the
    /// <c>/set-password</c> link. Does NOT send email — used when SMTP is unavailable and the
    /// admin will hand the link over manually.
    /// </summary>
    Task<string> IssueLinkAsync(User user, PasswordSetupKind kind, CancellationToken cancellationToken = default);

    /// <summary>Emails a previously-issued set-password <paramref name="link"/> to the user.</summary>
    Task SendAsync(User user, PasswordSetupKind kind, string link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience for the common case: issue a token and email the link. Used by admin
    /// invite/reset and self-service forgot-password. Does not check SMTP configuration —
    /// callers that need a hard failure should check first.
    /// </summary>
    Task IssueAndSendAsync(User user, PasswordSetupKind kind, CancellationToken cancellationToken = default);
}
