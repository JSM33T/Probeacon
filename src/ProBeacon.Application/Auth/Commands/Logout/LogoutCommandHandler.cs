using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<LogoutCommand>
{
    public async ValueTask<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var session = await db.UserSessions
            .FirstOrDefaultAsync(
                s => s.Id == currentUser.SessionId && s.UserId == currentUser.UserId,
                cancellationToken);

        if (session is not null && !session.IsRevoked)
        {
            session.Revoke();
            await db.SaveChangesAsync(cancellationToken);
        }

        return default;
    }
}
