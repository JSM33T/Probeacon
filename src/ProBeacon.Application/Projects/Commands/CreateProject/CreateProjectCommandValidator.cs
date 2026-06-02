using FluentValidation;

namespace ProBeacon.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(command => command.Name)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Project name is required.")
            .MaximumLength(100);

        RuleFor(command => command.Description)
            .MaximumLength(500);
    }
}
