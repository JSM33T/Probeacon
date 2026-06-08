using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Commands.RemoveProjectMember;

public class RemoveProjectMemberCommandHandler(
    IApplicationDbContext db,
    IProjectAccessService projectAccess)
    : ICommandHandler<RemoveProjectMemberCommand>
{
    public async ValueTask<Unit> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        // Project Managers or global Admins only. Also throws 404 if the project isn't in the tenant.
        await projectAccess.EnsureCanManageAsync(request.ProjectId, cancellationToken);

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
