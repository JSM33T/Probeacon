using Mediator;
using ProBeacon.Application.Users;

namespace ProBeacon.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;
