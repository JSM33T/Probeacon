namespace ProBeacon.Domain.Entities;

public class TenantSetting
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    public Tenant Tenant { get; private set; } = null!;

    private TenantSetting() { }

    public static TenantSetting Create(Guid tenantId, string key, string value) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Key = key,
        Value = value,
        UpdatedAt = DateTime.UtcNow
    };

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
