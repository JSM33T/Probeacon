using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Application.Users.Commands.PromoteToAdmin;

namespace ProBeacon.Api.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    [HttpPost("{userId:guid}/promote")]
    public async Task<IActionResult> Promote(Guid userId, CancellationToken cancellationToken)
    {
        await Sender.Send(new PromoteToAdminCommand(userId), cancellationToken);
        return NoContent();
    }
}
