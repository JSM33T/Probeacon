using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Application.Settings.Commands.UpsertSetting;
using ProBeacon.Application.Settings.Queries.GetSettings;

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
}
