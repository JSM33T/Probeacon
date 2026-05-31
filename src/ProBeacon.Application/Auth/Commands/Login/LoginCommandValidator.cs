using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(command => command.Password)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Password is required.");
    }
}
