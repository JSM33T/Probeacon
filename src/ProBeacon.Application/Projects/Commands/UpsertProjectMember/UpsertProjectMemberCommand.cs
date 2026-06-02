using Mediator;

namespace ProBeacon.Application.Projects.Commands.UpsertProjectMember;

public record UpsertProjectMemberCommand(Guid ProjectId, Guid UserId, string Role) : IRequest<ProjectMemberDto>;
