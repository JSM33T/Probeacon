using FluentValidation;

namespace ProBeacon.Application.Users.Commands.ReactivateUser;

public class ReactivateUserCommandValidator : AbstractValidator<ReactivateUserCommand>
{
    public ReactivateUserCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
