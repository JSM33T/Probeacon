namespace ProBeacon.Application.Common.Interfaces;

public interface IProjectAccessService
{
    Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default);

    /// <summary>Any project role (Viewer+) or a global Admin.</summary>
    Task EnsureCanViewAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>Project Editor/Manager or a global Admin.</summary>
    Task EnsureCanEditAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>Project Manager or a global Admin — may manage this project's members.</summary>
    Task EnsureCanManageAsync(Guid projectId, CancellationToken cancellationToken = default);
}
