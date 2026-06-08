using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Projects.Queries.GetProjectMembers;

public class GetProjectMembersQueryHandler(
    IApplicationDbContext db,
    IProjectAccessService projectAccess)
    : IRequestHandler<GetProjectMembersQuery, IReadOnlyList<ProjectMemberDto>>
{
    public async ValueTask<IReadOnlyList<ProjectMemberDto>> Handle(
        GetProjectMembersQuery request,
        CancellationToken cancellationToken)
    {
        // Managing members (and seeing the member list) is for project Managers or global Admins.
        await projectAccess.EnsureCanManageAsync(request.ProjectId, cancellationToken);

        return await db.ProjectMembers
            .Where(member => member.ProjectId == request.ProjectId)
            .OrderBy(member => member.User.DisplayName)
            .Select(member => new ProjectMemberDto(
                member.UserId,
                member.User.Email,
                member.User.DisplayName,
                member.User.IsActive,
                member.Role == ProjectRole.Manager ? "Manager"
                    : member.Role == ProjectRole.Editor ? "Editor"
                    : "Viewer",
                member.AssignedAt,
                member.AssignedByUserId))
            .ToListAsync(cancellationToken);
    }
}
