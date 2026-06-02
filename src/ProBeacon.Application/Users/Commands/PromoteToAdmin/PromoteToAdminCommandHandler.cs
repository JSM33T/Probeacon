using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Users.Commands.PromoteToAdmin;

public class PromoteToAdminCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<PromoteToAdminCommand>
{
    public async ValueTask<Unit> Handle(PromoteToAdminCommand request, CancellationToken cancellationToken)
    {
        var target = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == request.UserId && u.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        target.SetRole(UserRole.Admin);
        await db.SaveChangesAsync(cancellationToken);

        return default;
    }
}
