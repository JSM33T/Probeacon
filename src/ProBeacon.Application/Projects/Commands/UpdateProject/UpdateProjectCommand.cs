using Mediator;

namespace ProBeacon.Application.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(Guid ProjectId, string Name, string? Description) : IRequest<ProjectDto>;
