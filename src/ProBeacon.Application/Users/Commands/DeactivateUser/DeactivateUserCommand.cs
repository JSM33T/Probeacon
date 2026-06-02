using Mediator;

namespace ProBeacon.Application.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : ICommand;
