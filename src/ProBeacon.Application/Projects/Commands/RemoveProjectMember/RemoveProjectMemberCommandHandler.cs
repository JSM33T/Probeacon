using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Commands.RemoveProjectMember;

public class RemoveProjectMemberCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<RemoveProjectMemberCommand>
{
    public async ValueTask<Unit> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await db.Projects.AnyAsync(
            project => project.Id == request.ProjectId && project.TenantId == currentUser.TenantId,
            cancellationToken);

        if (!projectExists)
            throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(
                member => member.ProjectId == request.ProjectId && member.UserId == request.UserId,
                cancellationToken);

        if (member is not null)
            db.ProjectMembers.Remove(member);

        await db.SaveChangesAsync(cancellationToken);
        return default;
    }
}
