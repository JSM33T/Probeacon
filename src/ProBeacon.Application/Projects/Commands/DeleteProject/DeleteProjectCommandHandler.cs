using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<DeleteProjectCommand>
{
    public async ValueTask<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.ProjectId && project.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
        return default;
    }
}
