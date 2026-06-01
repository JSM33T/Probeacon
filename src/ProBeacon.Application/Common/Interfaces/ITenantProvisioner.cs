using ProBeacon.Application.Common.Models;

namespace ProBeacon.Application.Common.Interfaces;

public interface ITenantProvisioner
{
    Task<TenantProvisioningResult> ProvisionAsync(
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default);
}
