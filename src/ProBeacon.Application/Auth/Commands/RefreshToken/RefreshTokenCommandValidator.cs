using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Refresh token is required.");
    }
}
