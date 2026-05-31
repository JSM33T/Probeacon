using FluentValidation;

namespace ProBeacon.Application.Users.Commands.PromoteToAdmin;

public class PromoteToAdminCommandValidator : AbstractValidator<PromoteToAdminCommand>
{
    public PromoteToAdminCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
