using Mediator;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Settings.Queries.GetLockoutSettings;

public class GetLockoutSettingsQueryHandler(ILockoutPolicyProvider provider, ICurrentUser currentUser)
    : IRequestHandler<GetLockoutSettingsQuery, LockoutSettingsDto>
{
    public async ValueTask<LockoutSettingsDto> Handle(GetLockoutSettingsQuery request, CancellationToken cancellationToken)
    {
        var policy = await provider.GetAsync(currentUser.TenantId, cancellationToken);
        return policy.ToDto();
    }
}
