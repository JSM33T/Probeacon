using FluentValidation;

namespace ProBeacon.Application.Users.Commands.ResetUserPassword;

public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
