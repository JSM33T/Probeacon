using Mediator;

namespace ProBeacon.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(Guid SessionId, string RefreshToken) : IRequest<RefreshResult>;
