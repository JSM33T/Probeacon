using Mediator;

namespace ProBeacon.Application.Users.Commands.ReactivateUser;

public record ReactivateUserCommand(Guid UserId) : ICommand;
