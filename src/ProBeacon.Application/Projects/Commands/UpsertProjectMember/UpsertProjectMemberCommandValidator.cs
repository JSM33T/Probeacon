using FluentValidation;

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
            .Must(role => role.Equals("Viewer", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Editor", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be Viewer or Editor.");
    }
}
