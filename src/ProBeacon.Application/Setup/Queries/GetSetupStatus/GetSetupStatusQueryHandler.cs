using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Setup.Queries.GetSetupStatus;

public class GetSetupStatusQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSetupStatusQuery, bool>
{
    public async ValueTask<bool> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
        => await db.Tenants.AnyAsync(cancellationToken);
}
