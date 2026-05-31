using Mediator;

namespace ProBeacon.Application.Auth.Commands.TestSmtp;

public record TestSmtpResult(bool Success, string Message);

public record TestSmtpCommand : IRequest<TestSmtpResult>;
