using Mediator;

namespace ProBeacon.Application.Users.Commands.PromoteToAdmin;

public record PromoteToAdminCommand(Guid UserId) : ICommand;
