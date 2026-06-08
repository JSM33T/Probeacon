using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;

namespace ProBeacon.Application.Auth.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandHandler(
    IApplicationDbContext db,
    IPasswordSetupMailer mailer)
    : ICommandHandler<RequestPasswordResetCommand>
{
    public async ValueTask<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

        // Only send when the account exists, but always return success either way so the
        // response can't be used to discover which emails are registered.
        if (user is not null)
            await mailer.IssueAndSendAsync(user, PasswordSetupKind.Reset, cancellationToken);

        return default;
    }
}
