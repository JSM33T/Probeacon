using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Application.Auth.Commands.Login;
using ProBeacon.Application.Auth.Commands.Logout;
using ProBeacon.Application.Auth.Commands.RefreshToken;
using ProBeacon.Application.Auth.Commands.RevokeSession;
using ProBeacon.Application.Auth.Queries.GetSessions;

namespace ProBeacon.Api.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await Sender.Send(new LogoutCommand(), cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetSessionsQuery(), cancellationToken));

    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        await Sender.Send(new RevokeSessionCommand(sessionId), cancellationToken);
        return NoContent();
    }
}
