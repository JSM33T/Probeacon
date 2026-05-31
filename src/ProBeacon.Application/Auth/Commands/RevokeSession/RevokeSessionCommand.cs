using Mediator;

namespace ProBeacon.Application.Auth.Commands.RevokeSession;

public record RevokeSessionCommand(Guid SessionId) : ICommand;
