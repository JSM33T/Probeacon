using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Users.Commands.DemoteToMember;

public class DemoteToMemberCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<DemoteToMemberCommand>
{
    public async ValueTask<Unit> Handle(DemoteToMemberCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == currentUser.UserId)
            throw new InvalidOperationException("You cannot demote your own account.");

        var target = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == request.UserId && u.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (target.Role != UserRole.Admin)
            return default; // already a member — nothing to do

        // Never leave the workspace without an active admin to manage it.
        var otherActiveAdmins = await db.Users.CountAsync(
            u => u.TenantId == currentUser.TenantId
                && u.Role == UserRole.Admin
                && u.IsActive
                && u.Id != target.Id,
            cancellationToken);

        if (otherActiveAdmins == 0)
            throw new ConflictException(
                "You can't demote the last admin — promote another active user to admin first.");

        target.SetRole(UserRole.Member);
        await db.SaveChangesAsync(cancellationToken);

        return default;
    }
}
