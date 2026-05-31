using Mediator;

namespace ProBeacon.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : ICommand;
