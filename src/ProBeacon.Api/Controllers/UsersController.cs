using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Api.Authorization;
using ProBeacon.Application.Users.Commands.CreateUser;
using ProBeacon.Application.Users.Commands.DeactivateUser;
using ProBeacon.Application.Users.Commands.PromoteToAdmin;
using ProBeacon.Application.Users.Commands.ReactivateUser;
using ProBeacon.Application.Users.Commands.ResetUserPassword;
using ProBeacon.Application.Users.Commands.UpdateProfile;
using ProBeacon.Application.Users.Queries.GetProfile;
using ProBeacon.Application.Users.Queries.GetUsers;

namespace ProBeacon.Api.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await Sender.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

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

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ResetUserPasswordCommand(userId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{userId:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid userId, CancellationToken cancellationToken)
    {
        await Sender.Send(new ReactivateUserCommand(userId), cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeactivateUserCommand(userId), cancellationToken);
        return NoContent();
    }
}
