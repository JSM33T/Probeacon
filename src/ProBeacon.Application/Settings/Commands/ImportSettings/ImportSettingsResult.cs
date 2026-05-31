namespace ProBeacon.Application.Settings.Commands.ImportSettings;

public record SkippedSetting(string Key, string Reason);

public class ImportSettingsResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int SecretsPreserved { get; set; }
    public int Deleted { get; set; }
    public List<SkippedSetting> Skipped { get; set; } = new();
}
