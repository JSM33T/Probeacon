using ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Common.Interfaces;

public interface IPasswordSetupMailer
{
    /// <summary>
    /// Issues a single-use set-password token for the user, persists it, and emails them a
    /// link to <c>/set-password</c>. Used by admin invite/reset and self-service forgot-password.
    /// Does not check SMTP configuration — callers that need a hard failure should check first.
    /// </summary>
    Task IssueAndSendAsync(User user, PasswordSetupKind kind, CancellationToken cancellationToken = default);
}
