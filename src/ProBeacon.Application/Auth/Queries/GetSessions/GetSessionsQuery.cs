using Mediator;

namespace ProBeacon.Application.Auth.Queries.GetSessions;

public record GetSessionsQuery : IRequest<List<SessionDto>>;
