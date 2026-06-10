using FluentValidation;

namespace ProBeacon.Application.Common.Validation;

/// <summary>
/// Single source of truth for the password policy, reused by every flow that accepts a
/// user-chosen password (setup, signup, set/reset via link, profile change). Not applied to
/// login — that only verifies against the stored hash, so existing passwords keep working.
/// </summary>
public static class PasswordRules
{
    public const int MinLength = 10;

    // bcrypt silently truncates input beyond 72 bytes, so longer passwords are meaningless.
    public const int MaxLength = 72;

    public static IRuleBuilderOptions<T, string?> Password<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(MinLength).WithMessage($"Password must be at least {MinLength} characters.")
            .MaximumLength(MaxLength).WithMessage($"Password must be at most {MaxLength} characters.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");
}
