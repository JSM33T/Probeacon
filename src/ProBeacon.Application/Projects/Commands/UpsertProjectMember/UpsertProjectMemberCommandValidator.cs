using FluentValidation;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Projects.Commands.UpsertProjectMember;

public class UpsertProjectMemberCommandValidator : AbstractValidator<UpsertProjectMemberCommand>
{
    public UpsertProjectMemberCommandValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Role)
            .Must(role => Enum.TryParse<ProjectRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be Viewer, Editor, or Manager.");
    }
}
