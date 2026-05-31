using FluentValidation;

namespace ProBeacon.Application.Setup.Commands;

public class SetupCommandValidator : AbstractValidator<SetupCommand>
{
    public SetupCommandValidator()
    {
        RuleFor(command => command.OrgName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Organization name is required.");

        RuleFor(command => command.AdminName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Admin name is required.");

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
