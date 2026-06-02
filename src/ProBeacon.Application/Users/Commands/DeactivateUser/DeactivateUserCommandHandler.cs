using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<DeactivateUserCommand>
{
    public async ValueTask<Unit> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == currentUser.UserId)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        var target = await db.Users
            .FirstOrDefaultAsync(
                user => user.Id == request.UserId && user.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        target.Deactivate();

        var sessions = await db.UserSessions
            .Where(session => session.UserId == target.Id && !session.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
            session.Revoke();

        await db.SaveChangesAsync(cancellationToken);
        return default;
    }
}
