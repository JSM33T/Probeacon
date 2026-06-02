using Mediator;

namespace ProBeacon.Application.Projects.Queries.GetProjectMembers;

public record GetProjectMembersQuery(Guid ProjectId) : IRequest<IReadOnlyList<ProjectMemberDto>>;
