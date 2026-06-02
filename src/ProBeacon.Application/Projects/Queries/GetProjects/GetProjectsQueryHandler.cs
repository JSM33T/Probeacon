using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IProjectAccessService projectAccess)
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>
{
    public async ValueTask<IReadOnlyList<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (await projectAccess.IsCurrentUserAdminAsync(cancellationToken))
        {
            return await db.Projects
                .Where(project => project.TenantId == currentUser.TenantId)
                .OrderBy(project => project.Name)
                .Select(project => new ProjectDto(
                    project.Id,
                    project.Name,
                    project.Description,
                    project.CreatedAt,
                    project.CreatedByUserId,
                    "Full access",
                    project.Members.Count(member => member.User.IsActive)))
                .ToListAsync(cancellationToken);
        }

        return await db.ProjectMembers
            .Where(member =>
                member.UserId == currentUser.UserId
                && member.User.IsActive
                && member.Project.TenantId == currentUser.TenantId
                && member.CanView)
            .OrderBy(member => member.Project.Name)
            .Select(member => new ProjectDto(
                member.Project.Id,
                member.Project.Name,
                member.Project.Description,
                member.Project.CreatedAt,
                member.Project.CreatedByUserId,
                member.CanEdit ? "Editor" : "Viewer",
                member.Project.Members.Count(projectMember => projectMember.User.IsActive)))
            .ToListAsync(cancellationToken);
    }
}
