using Mediator;

namespace ProBeacon.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string DisplayName,
    string Role
) : IRequest<CreateUserResult>;
