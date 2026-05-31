using Mediator;

namespace ProBeacon.Application.Setup.Commands;

public record SetupCommand(
    string OrgName,
    string AdminName,
    string Email,
    string Password
) : IRequest<SetupResult>;
