using Mediator;

namespace ProBeacon.Application.Auth.Commands.RequestPasswordReset;

/// <summary>
/// Public, self-service "forgot password": emails a reset link to the address if an active
/// account exists. Always succeeds regardless, to avoid revealing which emails are registered.
/// </summary>
public record RequestPasswordResetCommand(string Email) : ICommand;
