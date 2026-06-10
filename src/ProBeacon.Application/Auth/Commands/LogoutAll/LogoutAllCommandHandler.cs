using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.LogoutAll;

public class LogoutAllCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<LogoutAllCommand>
{
    public async ValueTask<Unit> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
    {
        // Revoke every active session for the current user — signs out all devices, including
        // this one (the caller clears its own client state and the refresh cookie is cleared).
        var sessions = await db.UserSessions
            .Where(session => session.UserId == currentUser.UserId && !session.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
            session.Revoke();

        await db.SaveChangesAsync(cancellationToken);
        return default;
    }
}
