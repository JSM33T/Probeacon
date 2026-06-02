using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    public async ValueTask<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
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

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt,
            project.CreatedByUserId,
            "Admin",
            memberCount);
    }
}
