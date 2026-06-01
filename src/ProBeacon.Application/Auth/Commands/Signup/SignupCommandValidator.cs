using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.Signup;

public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(command => command.OrgName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Workspace name is required.");

        RuleFor(command => command.AdminName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Your name is required.");

        RuleFor(command => command.Email)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(command => command.Password)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Password is required.")
            .MinimumLength(8);
    }
}
