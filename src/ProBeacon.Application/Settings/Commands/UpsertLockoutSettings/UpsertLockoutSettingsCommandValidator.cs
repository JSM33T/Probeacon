using FluentValidation;
using ProBeacon.Application.Common.Models;

namespace ProBeacon.Application.Settings.Commands.UpsertLockoutSettings;

public class UpsertLockoutSettingsCommandValidator : AbstractValidator<UpsertLockoutSettingsCommand>
{
    public UpsertLockoutSettingsCommandValidator()
    {
        RuleFor(x => x.MaxAttempts)
            .InclusiveBetween(LockoutLimits.MinAttempts, LockoutLimits.MaxAttempts);

        RuleFor(x => x.BaseMinutes)
            .InclusiveBetween(LockoutLimits.MinMinutes, LockoutLimits.MaxMinutes);

        RuleFor(x => x.MaxMinutes)
            .InclusiveBetween(LockoutLimits.MinMinutes, LockoutLimits.MaxMinutes)
            .GreaterThanOrEqualTo(x => x.BaseMinutes)
            .WithMessage("Max lockout must be at least the base lockout.");
    }
}
