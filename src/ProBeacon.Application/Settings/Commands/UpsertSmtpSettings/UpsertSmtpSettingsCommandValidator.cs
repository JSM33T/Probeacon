using FluentValidation;

namespace ProBeacon.Application.Settings.Commands.UpsertSmtpSettings;

public class UpsertSmtpSettingsCommandValidator : AbstractValidator<UpsertSmtpSettingsCommand>
{
    public UpsertSmtpSettingsCommandValidator()
    {
        RuleFor(command => command.Host)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("SMTP host is required.")
            .Matches(@"^[\w\-\.]+$")
            .WithMessage("SMTP host is invalid.");

        RuleFor(command => command.Port)
            .InclusiveBetween(1, 65535);

        RuleFor(command => command.FromAddress)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("From address is required.")
            .EmailAddress();

        When(command => command.FromName is not null, () =>
        {
            RuleFor(command => command.FromName)
                .Must(value => value is null || value.Length == 0 || !string.IsNullOrWhiteSpace(value))
                .WithMessage("From name cannot contain only whitespace.");
        });
    }
}
