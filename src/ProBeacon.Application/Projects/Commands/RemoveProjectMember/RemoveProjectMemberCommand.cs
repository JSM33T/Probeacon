using Mediator;

namespace ProBeacon.Application.Projects.Commands.RemoveProjectMember;

public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : ICommand;
