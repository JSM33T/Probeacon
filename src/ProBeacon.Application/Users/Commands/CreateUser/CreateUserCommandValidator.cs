using FluentValidation;

namespace ProBeacon.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(command => command.DisplayName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Display name is required.");

        RuleFor(command => command.Role)
            .Must(role => Enum.TryParse<ProBeacon.Domain.Enums.UserRole>(role, true, out _))
            .WithMessage("Role must be Admin or Member.");
    }
}
