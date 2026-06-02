using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Users.Commands.ReactivateUser;

public class ReactivateUserCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<ReactivateUserCommand>
{
    public async ValueTask<Unit> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var target = await db.Users
            .FirstOrDefaultAsync(
                user => user.Id == request.UserId && user.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        target.Activate();
        await db.SaveChangesAsync(cancellationToken);
        return default;
    }
}
