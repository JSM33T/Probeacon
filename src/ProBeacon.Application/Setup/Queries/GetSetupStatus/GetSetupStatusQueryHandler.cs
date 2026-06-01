using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Options;

namespace ProBeacon.Application.Setup.Queries.GetSetupStatus;

public class GetSetupStatusQueryHandler(
    IApplicationDbContext db,
    IOptions<AppOptions> appOptions,
    IOptions<DemoOptions> demoOptions)
    : IRequestHandler<GetSetupStatusQuery, SetupStatusResult>
{
    public async ValueTask<SetupStatusResult> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
        => new(
            await db.Tenants.AnyAsync(cancellationToken),
            appOptions.Value.DeploymentMode.ToString(),
            demoOptions.Value.WorkspaceLifetimeHours);
}
