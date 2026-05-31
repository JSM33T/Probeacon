namespace ProBeacon.Application.Settings.Queries.ExportSettings;

public class SettingsExport
{
    public int Version { get; init; } = 1;
    public DateTime ExportedAt { get; init; }
    public bool IncludesSecrets { get; init; }

    /// <summary>Flat key/value map (dotted keys kept verbatim — no nesting).</summary>
    public Dictionary<string, string> Settings { get; init; } = new();

    /// <summary>Secret keys whose values were redacted (only set when secrets excluded).</summary>
    public List<string> RedactedKeys { get; init; } = new();
}
