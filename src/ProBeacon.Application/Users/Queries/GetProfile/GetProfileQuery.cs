using Mediator;
using ProBeacon.Application.Users.Queries.GetProfile;

namespace ProBeacon.Application.Users.Queries.GetProfile;

public record GetProfileQuery : IRequest<ProfileDto>;
