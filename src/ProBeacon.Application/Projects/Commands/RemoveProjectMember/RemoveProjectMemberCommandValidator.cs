using FluentValidation;

namespace ProBeacon.Application.Projects.Commands.RemoveProjectMember;

public class RemoveProjectMemberCommandValidator : AbstractValidator<RemoveProjectMemberCommand>
{
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
