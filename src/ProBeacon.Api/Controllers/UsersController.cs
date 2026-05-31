using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Api.Authorization;
using ProBeacon.Application.Users.Commands.PromoteToAdmin;
using ProBeacon.Application.Users.Commands.UpdateProfile;
using ProBeacon.Application.Users.Queries.GetProfile;

namespace ProBeacon.Api.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var profile = await Sender.Send(new GetProfileQuery(), cancellationToken);
        return Ok(profile);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await Sender.Send(command, cancellationToken);
        return Ok(profile);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{userId:guid}/promote")]
    public async Task<IActionResult> Promote(Guid userId, CancellationToken cancellationToken)
    {
        await Sender.Send(new PromoteToAdminCommand(userId), cancellationToken);
        return NoContent();
    }
}
