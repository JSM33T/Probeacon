using System.Collections;
using System.Text;
using ProBeacon.Application.Settings.Queries.ExportSettings;
using YamlDotNet.Serialization;

namespace ProBeacon.Api.Services;

/// <summary>
/// Serializes settings to / from a flat-key YAML document. Dotted keys are kept
/// verbatim (no nesting) so the file round-trips losslessly.
/// </summary>
public static class SettingsYaml
{
    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithQuotingNecessaryStrings()
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly HashSet<string> MetadataKeys =
        new(StringComparer.OrdinalIgnoreCase) { "version", "exportedAt", "includesSecrets" };

    public static string Serialize(SettingsExport export)
    {
        var body = Serializer.Serialize(new
        {
            version = export.Version,
            exportedAt = export.ExportedAt.ToString("O"),
            includesSecrets = export.IncludesSecrets,
            settings = export.Settings,
        });

        var sb = new StringBuilder();
        sb.AppendLine("# ProBeacon settings export");
        sb.AppendLine($"# Generated {export.ExportedAt:O}");
        sb.AppendLine(export.IncludesSecrets
            ? "# Secrets: INCLUDED in plaintext — store and share this file carefully."
            : "# Secrets: redacted. Re-importing preserves the existing stored values.");
        if (export.RedactedKeys.Count > 0)
            sb.AppendLine($"# Redacted keys: {string.Join(", ", export.RedactedKeys)}");
        sb.AppendLine();
        sb.Append(body);
        return sb.ToString();
    }

    /// <summary>
    /// Parses a YAML document into a flat key/value map. Accepts either a document
    /// with a <c>settings:</c> map, or a bare flat map of keys (metadata keys ignored).
    /// </summary>
    public static Dictionary<string, string> Parse(string content)
    {
        var raw = Deserializer.Deserialize<Dictionary<string, object?>>(content);
        if (raw is null)
            return new Dictionary<string, string>();

        IEnumerable<KeyValuePair<string, object?>> entries =
            raw.TryGetValue("settings", out var settingsObj) && settingsObj is IDictionary nested
                ? Flatten(nested)
                : raw.Where(kv => !MetadataKeys.Contains(kv.Key));

        var result = new Dictionary<string, string>();
        foreach (var (key, value) in entries)
            if (!string.IsNullOrWhiteSpace(key))
                result[key] = value?.ToString() ?? string.Empty;
        return result;
    }

    private static IEnumerable<KeyValuePair<string, object?>> Flatten(IDictionary dict)
    {
        foreach (DictionaryEntry entry in dict)
            yield return new KeyValuePair<string, object?>(entry.Key?.ToString() ?? string.Empty, entry.Value);
    }
}
