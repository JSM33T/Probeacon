using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Projects.Commands.UpsertProjectMember;

public class UpsertProjectMemberCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IProjectAccessService projectAccess)
    : IRequestHandler<UpsertProjectMemberCommand, ProjectMemberDto>
{
    public async ValueTask<ProjectMemberDto> Handle(
        UpsertProjectMemberCommand request,
        CancellationToken cancellationToken)
    {
        // Project Managers or global Admins only. Also throws 404 if the project isn't in the tenant.
        await projectAccess.EnsureCanManageAsync(request.ProjectId, cancellationToken);

        var role = Enum.Parse<ProjectRole>(request.Role, ignoreCase: true);

        var user = await db.Users
            .FirstOrDefaultAsync(
                user => user.Id == request.UserId && user.TenantId == currentUser.TenantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (!user.IsActive)
            throw new InvalidOperationException("Inactive users cannot be assigned to projects.");

        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(
                member => member.ProjectId == request.ProjectId && member.UserId == request.UserId,
                cancellationToken);

        if (member is null)
        {
            member = ProjectMember.Create(request.ProjectId, request.UserId, role, currentUser.UserId);
            db.ProjectMembers.Add(member);
        }
        else
        {
            member.SetRole(role);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            role.ToString(),
            member.AssignedAt,
            member.AssignedByUserId);
    }
}
