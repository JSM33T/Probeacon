using Mediator;
using ProBeacon.Application.Users.Queries.GetProfile;

namespace ProBeacon.Application.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string? DisplayName,
    string? Email,
    string? CurrentPassword,
    string? NewPassword
) : IRequest<ProfileDto>;
