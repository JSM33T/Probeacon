using FluentValidation;

namespace ProBeacon.Application.Settings.Commands.DeleteSetting;

public class DeleteSettingCommandValidator : AbstractValidator<DeleteSettingCommand>
{
    private const string SettingKeyPattern = @"^[a-z][a-z0-9]*(\.[a-z0-9][a-z0-9_-]*)*$";

    public DeleteSettingCommandValidator()
    {
        RuleFor(command => command.Key)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Setting key is required.")
            .Matches(SettingKeyPattern)
            .WithMessage("Setting key must use lowercase dotted format.");
    }
}
