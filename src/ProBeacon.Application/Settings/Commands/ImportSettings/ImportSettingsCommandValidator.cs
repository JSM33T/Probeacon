using FluentValidation;

namespace ProBeacon.Application.Settings.Commands.ImportSettings;

public class ImportSettingsCommandValidator : AbstractValidator<ImportSettingsCommand>
{
    private const string SettingKeyPattern = @"^[a-z][a-z0-9]*(\.[a-z0-9][a-z0-9_-]*)*$";

    public ImportSettingsCommandValidator()
    {
        RuleFor(command => command.Settings)
            .NotNull()
            .Must(settings => settings.Count > 0)
            .WithMessage("At least one setting is required.");

        RuleForEach(command => command.Settings)
            .Must(entry => !string.IsNullOrWhiteSpace(entry.Key))
            .WithMessage("Setting key is required.")
            .Must(entry => entry.Key.Trim().Contains('.') || entry.Key.Trim().Length > 0)
            .Matches(entry => entry.Key, SettingKeyPattern)
            .WithMessage("Setting key must use lowercase dotted format.");
    }
}
