using System.Text.RegularExpressions;

namespace ProBeacon.Domain.Entities;

public class TenantSetting
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? ValidationRegex { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Tenant Tenant { get; private set; } = null!;

    private TenantSetting() { }

    public static TenantSetting Create(Guid tenantId, string key, string value, string? validationRegex = null)
    {
        var setting = new TenantSetting
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Key = key,
            ValidationRegex = validationRegex,
        };
        setting.UpdateValue(value);
        return setting;
    }

    public void SetValidationRegex(string? regex)
    {
        ValidationRegex = regex;
    }

    public void UpdateValue(string value)
    {
        if (ValidationRegex is not null && !Regex.IsMatch(value, ValidationRegex))
            throw new ArgumentException($"'{value}' is not a valid value for '{Key}'.");

        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
