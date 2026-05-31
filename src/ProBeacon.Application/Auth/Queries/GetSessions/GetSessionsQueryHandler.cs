using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Queries.GetSessions;

public class GetSessionsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetSessionsQuery, List<SessionDto>>
{
    public async ValueTask<List<SessionDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await db.UserSessions
            .Where(s => s.UserId == currentUser.UserId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActiveAt)
            .Select(s => new SessionDto(
                s.Id,
                s.UserAgent,
                s.IpAddress,
                s.CreatedAt,
                s.LastActiveAt,
                s.Id == currentUser.SessionId
            ))
            .ToListAsync(cancellationToken);

        return sessions;
    }
}
