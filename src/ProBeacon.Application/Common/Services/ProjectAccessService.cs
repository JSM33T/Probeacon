using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Common.Services;

public class ProjectAccessService(IApplicationDbContext db, ICurrentUser currentUser)
    : IProjectAccessService
{
    public async Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default)
        => await db.Users.AnyAsync(
            user =>
                user.Id == currentUser.UserId
                && user.TenantId == currentUser.TenantId
                && user.IsActive
                && user.Role == UserRole.Admin,
            cancellationToken);

    public Task EnsureCanViewAsync(Guid projectId, CancellationToken cancellationToken = default)
        => EnsureAccessAsync(projectId, ProjectRole.Viewer, cancellationToken);

    public Task EnsureCanEditAsync(Guid projectId, CancellationToken cancellationToken = default)
        => EnsureAccessAsync(projectId, ProjectRole.Editor, cancellationToken);

    public Task EnsureCanManageAsync(Guid projectId, CancellationToken cancellationToken = default)
        => EnsureAccessAsync(projectId, ProjectRole.Manager, cancellationToken);

    private async Task EnsureAccessAsync(Guid projectId, ProjectRole minimumRole, CancellationToken cancellationToken)
    {
        var projectExists = await db.Projects.AnyAsync(
            project => project.Id == projectId && project.TenantId == currentUser.TenantId,
            cancellationToken);

        if (!projectExists)
            throw new KeyNotFoundException($"Project {projectId} not found.");

        // Global admins have full access to every project regardless of membership.
        if (await IsCurrentUserAdminAsync(cancellationToken))
            return;

        var hasAccess = await db.ProjectMembers.AnyAsync(
            member =>
                member.ProjectId == projectId
                && member.UserId == currentUser.UserId
                && member.User.IsActive
                && member.Role >= minimumRole,
            cancellationToken);

        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have access to this project.");
    }
}
