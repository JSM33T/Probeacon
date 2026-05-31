using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(command => command.Token)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Verification token is required.");
    }
}
