using Mediator;

namespace ProBeacon.Application.Projects.Queries.GetProject;

public record GetProjectQuery(Guid ProjectId) : IRequest<ProjectDto>;
