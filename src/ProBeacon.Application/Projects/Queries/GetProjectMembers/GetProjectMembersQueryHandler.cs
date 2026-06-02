using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Queries.GetProjectMembers;

public class GetProjectMembersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetProjectMembersQuery, IReadOnlyList<ProjectMemberDto>>
{
    public async ValueTask<IReadOnlyList<ProjectMemberDto>> Handle(
        GetProjectMembersQuery request,
        CancellationToken cancellationToken)
    {
        var projectExists = await db.Projects.AnyAsync(
            project => project.Id == request.ProjectId && project.TenantId == currentUser.TenantId,
            cancellationToken);

        if (!projectExists)
            throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        return await db.ProjectMembers
            .Where(member => member.ProjectId == request.ProjectId)
            .OrderBy(member => member.User.DisplayName)
            .Select(member => new ProjectMemberDto(
                member.UserId,
                member.User.Email,
                member.User.DisplayName,
                member.User.IsActive,
                member.CanEdit ? "Editor" : "Viewer",
                member.AssignedAt,
                member.AssignedByUserId))
            .ToListAsync(cancellationToken);
    }
}
