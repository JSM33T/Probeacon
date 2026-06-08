using Mediator;

namespace ProBeacon.Application.Auth.Commands.SetPassword;

/// <summary>
/// Public: completes an invite/reset by setting a new password from a single-use token,
/// then signs the user in (returns a fresh session, same shape as login).
/// </summary>
public record SetPasswordCommand(string Token, string Password) : IRequest<LoginResult>;
