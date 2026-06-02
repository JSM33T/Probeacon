using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Services;

namespace ProBeacon.Application.Users.Commands.ResetUserPassword;

public class ResetUserPasswordCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ResetUserPasswordCommand, ResetUserPasswordResult>
{
    public async ValueTask<ResetUserPasswordResult> Handle(
        ResetUserPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var target = await db.Users
            .FirstOrDefaultAsync(
                user => user.Id == request.UserId && user.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        target.UpdatePasswordHash(passwordHasher.Hash(temporaryPassword));

        var sessions = await db.UserSessions
            .Where(session => session.UserId == target.Id && !session.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
            session.Revoke();

        await db.SaveChangesAsync(cancellationToken);
        return new ResetUserPasswordResult(temporaryPassword);
    }
}
