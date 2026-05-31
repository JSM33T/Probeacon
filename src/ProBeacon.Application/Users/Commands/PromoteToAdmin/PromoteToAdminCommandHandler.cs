using MediatR;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Users.Commands.PromoteToAdmin;

public class PromoteToAdminCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<PromoteToAdminCommand>
{
    public async Task Handle(PromoteToAdminCommand request, CancellationToken cancellationToken)
    {
        var caller = await db.Users
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (caller.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only admins can promote users.");

        var target = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        target.SetRole(UserRole.Admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}
