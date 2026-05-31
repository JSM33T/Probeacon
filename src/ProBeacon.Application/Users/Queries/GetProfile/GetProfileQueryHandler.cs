using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Users.Queries.GetProfile;

public class GetProfileQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetProfileQuery, ProfileDto>
{
    public async ValueTask<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        return new ProfileDto(user.Id, user.Email, user.DisplayName, user.Role.ToString());
    }
}
