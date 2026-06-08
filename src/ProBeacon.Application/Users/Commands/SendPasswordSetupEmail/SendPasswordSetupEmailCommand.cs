using Mediator;

namespace ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;

public enum PasswordSetupKind
{
    /// <summary>A newly created user setting their password for the first time.</summary>
    Invite,
    /// <summary>An existing user resetting their password.</summary>
    Reset
}

/// <summary>
/// Internal worker: issues a single-use set-password token for a user in the current tenant
/// and emails them a link. Dispatched by admin-initiated create-user (Invite) and
/// reset-password (Reset). Throws <see cref="Common.Exceptions.EmailNotConfiguredException"/>
/// when SMTP is unconfigured.
/// </summary>
public record SendPasswordSetupEmailCommand(Guid UserId, PasswordSetupKind Kind) : ICommand;
