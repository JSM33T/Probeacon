using Mediator;

namespace ProBeacon.Application.Users.Commands.ResetUserPassword;

public record ResetUserPasswordCommand(Guid UserId) : IRequest<ResetUserPasswordResult>;
