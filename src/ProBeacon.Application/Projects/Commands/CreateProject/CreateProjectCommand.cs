using Mediator;

namespace ProBeacon.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(string Name, string? Description) : IRequest<ProjectDto>;
