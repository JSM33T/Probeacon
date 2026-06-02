namespace ProBeacon.Application.Common.Interfaces;

public interface IProjectAccessService
{
    Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default);
    Task EnsureCanViewAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task EnsureCanEditAsync(Guid projectId, CancellationToken cancellationToken = default);
}
