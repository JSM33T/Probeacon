using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Projects.Queries.GetAssignableUsers;

public class GetAssignableUsersQueryHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IProjectAccessService projectAccess)
    : IRequestHandler<GetAssignableUsersQuery, IReadOnlyList<AssignableUserDto>>
{
    public async ValueTask<IReadOnlyList<AssignableUserDto>> Handle(
        GetAssignableUsersQuery request,
        CancellationToken cancellationToken)
    {
        // Only someone who can manage this project may enumerate the tenant's users to assign.
        await projectAccess.EnsureCanManageAsync(request.ProjectId, cancellationToken);

        return await db.Users
            .Where(user => user.TenantId == currentUser.TenantId && user.IsActive)
            .OrderBy(user => user.DisplayName)
            .Select(user => new AssignableUserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.IsActive))
            .ToListAsync(cancellationToken);
    }
}
