using Mediator;

namespace ProBeacon.Application.Auth.Commands.Signup;

public record SignupCommand(
    string OrgName,
    string AdminName,
    string Email,
    string Password
) : IRequest<SignupResult>;
