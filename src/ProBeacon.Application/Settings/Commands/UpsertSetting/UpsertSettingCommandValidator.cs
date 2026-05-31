using FluentValidation;

namespace ProBeacon.Application.Settings.Commands.UpsertSetting;

public class UpsertSettingCommandValidator : AbstractValidator<UpsertSettingCommand>
{
    private const string SettingKeyPattern = @"^[a-z][a-z0-9]*(\.[a-z0-9][a-z0-9_-]*)*$";

    public UpsertSettingCommandValidator()
    {
        RuleFor(command => command.Key)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Setting key is required.")
            .Matches(SettingKeyPattern)
            .WithMessage("Setting key must use lowercase dotted format.");

        RuleFor(command => command.Value)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Setting value is required.");
    }
}
