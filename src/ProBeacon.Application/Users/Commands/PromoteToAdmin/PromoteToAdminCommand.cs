using MediatR;

namespace ProBeacon.Application.Users.Commands.PromoteToAdmin;

public record PromoteToAdminCommand(Guid UserId) : IRequest;
