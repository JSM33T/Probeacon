using Mediator;

namespace ProBeacon.Application.Auth.Commands.SendVerificationEmail;

public record SendVerificationEmailCommand(Guid UserId) : ICommand;
