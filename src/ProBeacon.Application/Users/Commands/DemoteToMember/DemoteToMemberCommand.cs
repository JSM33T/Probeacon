using Mediator;

namespace ProBeacon.Application.Users.Commands.DemoteToMember;

public record DemoteToMemberCommand(Guid UserId) : ICommand;
