using Mediator;

namespace ProBeacon.Application.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<IReadOnlyList<ProjectDto>>;
