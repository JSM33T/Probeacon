using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.SetPassword;

public class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
{
    public SetPasswordCommandValidator()
    {
        RuleFor(command => command.Token)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Token is required.");

        RuleFor(command => command.Password)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Password is required.")
            .MinimumLength(8);
    }
}
