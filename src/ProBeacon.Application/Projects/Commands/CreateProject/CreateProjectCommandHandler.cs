using Mediator;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async ValueTask<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(
            currentUser.TenantId,
            request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            currentUser.UserId);

        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt,
            project.CreatedByUserId,
            "Admin",
            0);
    }
}
