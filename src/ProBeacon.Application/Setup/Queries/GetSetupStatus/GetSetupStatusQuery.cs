using MediatR;

namespace ProBeacon.Application.Setup.Queries.GetSetupStatus;

public record GetSetupStatusQuery : IRequest<bool>;
