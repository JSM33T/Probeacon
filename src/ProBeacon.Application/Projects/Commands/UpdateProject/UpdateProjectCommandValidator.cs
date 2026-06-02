using FluentValidation;

namespace ProBeacon.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Project name is required.")
            .MaximumLength(100);

        RuleFor(command => command.Description)
            .MaximumLength(500);
    }
}
