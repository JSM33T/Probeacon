namespace ProBeacon.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public ICollection<TenantSetting> Settings { get; private set; } = [];
    public ICollection<User> Users { get; private set; } = [];

    private Tenant() { }

    public static Tenant Create(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        CreatedAt = DateTime.UtcNow
    };
}
