using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Api.Authorization;
using ProBeacon.Api.Services;
using ProBeacon.Application.Auth.Commands.TestSmtp;
using ProBeacon.Application.Settings.Commands.DeleteSetting;
using ProBeacon.Application.Settings.Commands.ImportSettings;
using ProBeacon.Application.Settings.Commands.UpsertSetting;
using ProBeacon.Application.Settings.Commands.UpsertSmtpSettings;
using ProBeacon.Application.Settings.Queries.ExportSettings;
using ProBeacon.Application.Settings.Queries.GetSettings;
using ProBeacon.Application.Settings.Queries.GetSmtpSettings;

namespace ProBeacon.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class SettingsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetSettingsQuery(), cancellationToken));

    [HttpPut]
    public async Task<IActionResult> UpsertSetting(UpsertSettingCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteSetting(string key, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteSettingCommand(key), cancellationToken);
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] bool includeSecrets, CancellationToken cancellationToken)
    {
        var export = await Sender.Send(new ExportSettingsQuery(includeSecrets), cancellationToken);
        var yaml = SettingsYaml.Serialize(export);
        var bytes = Encoding.UTF8.GetBytes(yaml);
        var filename = $"probeacon-settings-{export.ExportedAt:yyyyMMdd-HHmmss}.yaml";
        return File(bytes, "application/x-yaml", filename);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportSettingsRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { message = "The file is empty." });

        Dictionary<string, string> settings;
        try
        {
            settings = SettingsYaml.Parse(request.Content);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Could not parse YAML: {ex.Message}" });
        }

        if (settings.Count == 0)
            return BadRequest(new { message = "No settings found in the file." });

        var result = await Sender.Send(new ImportSettingsCommand(settings, request.Replace), cancellationToken);
        return Ok(result);
    }

    [HttpGet("smtp")]
    public async Task<IActionResult> GetSmtp(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetSmtpSettingsQuery(), cancellationToken));

    [HttpPut("smtp")]
    public async Task<IActionResult> UpsertSmtp(UpsertSmtpSettingsCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [HttpPost("smtp/test")]
    public async Task<IActionResult> TestSmtp(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new TestSmtpCommand(), cancellationToken));
}

public record ImportSettingsRequest(string Content, bool Replace);
