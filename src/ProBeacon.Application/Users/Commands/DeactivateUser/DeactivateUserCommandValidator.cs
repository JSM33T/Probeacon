using FluentValidation;

namespace ProBeacon.Application.Users.Commands.DeactivateUser;

public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
