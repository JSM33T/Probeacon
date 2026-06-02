namespace ProBeacon.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public Tenant Tenant { get; private set; } = null!;
    public User CreatedBy { get; private set; } = null!;
    public ICollection<ProjectMember> Members { get; private set; } = [];

    private Project() { }

    public static Project Create(Guid tenantId, string name, string? description, Guid createdByUserId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Name = name,
        Description = description,
        CreatedAt = DateTime.UtcNow,
        CreatedByUserId = createdByUserId
    };

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
