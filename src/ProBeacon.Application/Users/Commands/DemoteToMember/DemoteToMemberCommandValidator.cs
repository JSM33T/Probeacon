using FluentValidation;

namespace ProBeacon.Application.Users.Commands.DemoteToMember;

public class DemoteToMemberCommandValidator : AbstractValidator<DemoteToMemberCommand>
{
    public DemoteToMemberCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
