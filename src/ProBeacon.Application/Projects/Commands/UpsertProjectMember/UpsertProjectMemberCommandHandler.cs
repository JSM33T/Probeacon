using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Projects.Commands.UpsertProjectMember;

public class UpsertProjectMemberCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpsertProjectMemberCommand, ProjectMemberDto>
{
    public async ValueTask<ProjectMemberDto> Handle(
        UpsertProjectMemberCommand request,
        CancellationToken cancellationToken)
    {
        var projectExists = await db.Projects.AnyAsync(
            project => project.Id == request.ProjectId && project.TenantId == currentUser.TenantId,
            cancellationToken);

        if (!projectExists)
            throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        var user = await db.Users
            .FirstOrDefaultAsync(
                user => user.Id == request.UserId && user.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (!user.IsActive)
            throw new InvalidOperationException("Inactive users cannot be assigned to projects.");

        var isEditor = request.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase);
        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(
                member => member.ProjectId == request.ProjectId && member.UserId == request.UserId,
                cancellationToken);

        if (member is null)
        {
            member = ProjectMember.Create(
                request.ProjectId,
                request.UserId,
                canView: true,
                canEdit: isEditor,
                currentUser.UserId);
            db.ProjectMembers.Add(member);
        }
        else
        {
            member.UpdatePermissions(canView: true, canEdit: isEditor);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            isEditor ? "Editor" : "Viewer",
            member.AssignedAt,
            member.AssignedByUserId);
    }
}
