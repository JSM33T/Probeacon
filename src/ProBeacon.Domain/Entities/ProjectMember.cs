namespace ProBeacon.Domain.Entities;

public class ProjectMember
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public bool CanView { get; private set; }
    public bool CanEdit { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid AssignedByUserId { get; private set; }

    public Project Project { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public User AssignedBy { get; private set; } = null!;

    private ProjectMember() { }

    public static ProjectMember Create(
        Guid projectId,
        Guid userId,
        bool canView,
        bool canEdit,
        Guid assignedByUserId) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        UserId = userId,
        CanView = canView,
        CanEdit = canEdit,
        AssignedAt = DateTime.UtcNow,
        AssignedByUserId = assignedByUserId
    };

    public void UpdatePermissions(bool canView, bool canEdit)
    {
        CanView = canView;
        CanEdit = canEdit;
    }
}
