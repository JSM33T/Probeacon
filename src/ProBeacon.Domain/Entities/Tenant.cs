using ProBeacon.Domain.Enums;

namespace ProBeacon.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public TenantKind Kind { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<TenantSetting> Settings { get; private set; } = [];
    public ICollection<User> Users { get; private set; } = [];

    private Tenant() { }

    public static Tenant Create(
        string name,
        string slug,
        TenantKind kind = TenantKind.SelfHosted,
        DateTime? expiresAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Kind = kind,
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow
    };

    public bool IsExpired(DateTime utcNow)
        => Kind == TenantKind.OnlineDemo && ExpiresAt.HasValue && ExpiresAt.Value <= utcNow;
}
