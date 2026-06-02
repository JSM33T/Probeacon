using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Queries.GetProject;

public class GetProjectQueryHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IProjectAccessService projectAccess)
    : IRequestHandler<GetProjectQuery, ProjectDto>
{
    public async ValueTask<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        await projectAccess.EnsureCanViewAsync(request.ProjectId, cancellationToken);
        var isAdmin = await projectAccess.IsCurrentUserAdminAsync(cancellationToken);

        return await db.Projects
            .Where(project => project.Id == request.ProjectId && project.TenantId == currentUser.TenantId)
            .Select(project => new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt,
                project.CreatedByUserId,
                isAdmin
                    ? "Full access"
                    : project.Members
                        .Where(member => member.UserId == currentUser.UserId && member.User.IsActive)
                        .Select(member => member.CanEdit ? "Editor" : "Viewer")
                        .FirstOrDefault() ?? "Viewer",
                project.Members.Count(member => member.User.IsActive)))
            .FirstAsync(cancellationToken);
    }
}
