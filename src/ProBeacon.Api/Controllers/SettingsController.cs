using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Application.Auth.Commands.TestSmtp;
using ProBeacon.Application.Settings.Commands.DeleteSetting;
using ProBeacon.Application.Settings.Commands.UpsertSetting;
using ProBeacon.Application.Settings.Commands.UpsertSmtpSettings;
using ProBeacon.Application.Settings.Queries.GetSettings;
using ProBeacon.Application.Settings.Queries.GetSmtpSettings;

namespace ProBeacon.Api.Controllers;

[Authorize]
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
