using Microsoft.AspNetCore.Mvc;
using ProBeacon.Api.Services;
using ProBeacon.Application.Setup.Commands;
using ProBeacon.Application.Setup.Queries.GetSetupStatus;

namespace ProBeacon.Api.Controllers;

public class SetupController(SetupState setupState) : ApiControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var configured = await Sender.Send(new GetSetupStatusQuery(), cancellationToken);
        return Ok(new { configured });
    }

    [HttpPost]
    public async Task<IActionResult> Setup(SetupCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        setupState.MarkConfigured();
        return Ok(result);
    }
}
