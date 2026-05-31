using MediatR;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.RevokeSession;

public class RevokeSessionCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<RevokeSessionCommand>
{
    public async Task Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await db.UserSessions
            .FirstOrDefaultAsync(
                s => s.Id == request.SessionId && s.UserId == currentUser.UserId,
                cancellationToken);

        if (session is null)
            throw new KeyNotFoundException("Session not found.");

        if (session.IsRevoked)
            return;

        session.Revoke();
        await db.SaveChangesAsync(cancellationToken);
    }
}
