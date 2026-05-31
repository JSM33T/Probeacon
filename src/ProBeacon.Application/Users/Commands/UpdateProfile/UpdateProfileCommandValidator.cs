using FluentValidation;

namespace ProBeacon.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command)
            .Must(command =>
                !string.IsNullOrWhiteSpace(command.DisplayName) ||
                !string.IsNullOrWhiteSpace(command.Email) ||
                !string.IsNullOrWhiteSpace(command.NewPassword))
            .WithMessage("At least one profile field must be supplied.");

        When(command => command.DisplayName is not null, () =>
        {
            RuleFor(command => command.DisplayName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Display name cannot be empty.");
        });

        When(command => command.Email is not null, () =>
        {
            RuleFor(command => command.Email)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Email cannot be empty.")
                .EmailAddress();
        });

        When(command => command.NewPassword is not null, () =>
        {
            RuleFor(command => command.NewPassword)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("New password cannot be empty.")
                .MinimumLength(8);

            RuleFor(command => command.CurrentPassword)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Current password is required to set a new password.");
        });
    }
}
