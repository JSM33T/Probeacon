using Mediator;

namespace ProBeacon.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshResult>;
