using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IProjectAccessService projectAccess)
    : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    public async ValueTask<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        // Admins or project Editors/Managers only — throws 404 if the project isn't in the
        // tenant, 401 if the caller has no edit access.
        await projectAccess.EnsureCanEditAsync(request.ProjectId, cancellationToken);
        var isAdmin = await projectAccess.IsCurrentUserAdminAsync(cancellationToken);

        var project = await db.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.ProjectId && project.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        project.Update(
            request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim());

        await db.SaveChangesAsync(cancellationToken);

        var memberCount = await db.ProjectMembers.CountAsync(
            member => member.ProjectId == project.Id && member.User.IsActive,
            cancellationToken);

        // For a non-admin editor, label with their actual project role (Editor or Manager).
        var accessRole = "Full access";
        if (!isAdmin)
        {
            var role = await db.ProjectMembers
                .Where(member => member.ProjectId == project.Id && member.UserId == currentUser.UserId)
                .Select(member => member.Role)
                .FirstOrDefaultAsync(cancellationToken);
            accessRole = role.ToString();
        }

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt,
            project.CreatedByUserId,
            accessRole,
            memberCount);
    }
}
