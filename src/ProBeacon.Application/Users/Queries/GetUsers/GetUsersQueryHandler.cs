using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    public async ValueTask<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => await db.Users
            .Where(user => user.TenantId == currentUser.TenantId)
            .OrderBy(user => user.DisplayName)
            .Select(user => new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Role.ToString(),
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt))
            .ToListAsync(cancellationToken);
}
