using ProBeacon.Application.Common.Models;

namespace ProBeacon.Application.Common.Interfaces;

public interface ILockoutPolicyProvider
{
    /// <summary>Loads the effective lockout policy for a tenant (defaults applied, values clamped).</summary>
    Task<LockoutPolicy> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
