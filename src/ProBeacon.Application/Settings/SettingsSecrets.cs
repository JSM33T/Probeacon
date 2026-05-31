using System.Text.RegularExpressions;

namespace ProBeacon.Application.Settings;

/// <summary>
/// Shared rules for handling settings during export/import: detecting secret-looking
/// keys and the placeholder written in place of a redacted secret value.
/// </summary>
public static partial class SettingsSecrets
{
    /// <summary>
    /// Placeholder written for a secret value when secrets are not exported. On import,
    /// a value equal to this means "keep the existing stored value".
    /// </summary>
    public const string RedactedValue = "<unchanged>";

    [GeneratedRegex(@"(password|secret|token|credential|private|api[_-]?key)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretKeyRegex();

    public static bool IsSecretKey(string key) => SecretKeyRegex().IsMatch(key);
}
