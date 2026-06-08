using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;

public class SendPasswordSetupEmailCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IEmailSender emailSender,
    IPasswordSetupMailer mailer)
    : ICommandHandler<SendPasswordSetupEmailCommand>
{
    public async ValueTask<Unit> Handle(SendPasswordSetupEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == request.UserId && u.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (!await emailSender.IsConfiguredAsync(user.TenantId, cancellationToken))
            throw new EmailNotConfiguredException();

        await mailer.IssueAndSendAsync(user, request.Kind, cancellationToken);
        return default;
    }
}
