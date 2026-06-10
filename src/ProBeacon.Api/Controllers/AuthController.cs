using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProBeacon.Api.RateLimiting;
using ProBeacon.Application.Auth.Commands.Login;
using ProBeacon.Application.Auth.Commands.Logout;
using ProBeacon.Application.Auth.Commands.LogoutAll;
using ProBeacon.Application.Auth.Commands.RefreshToken;
using ProBeacon.Application.Auth.Commands.RequestPasswordReset;
using ProBeacon.Application.Auth.Commands.RevokeSession;
using ProBeacon.Application.Auth.Commands.SendVerificationEmail;
using ProBeacon.Application.Auth.Commands.SetPassword;
using ProBeacon.Application.Auth.Commands.Signup;
using ProBeacon.Application.Auth.Commands.VerifyEmail;
using ProBeacon.Application.Auth.Queries.GetSessions;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Api.Controllers;

public class AuthController(ICurrentUser currentUser) : ApiControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        SetRefreshCookie(result.RefreshToken!);
        return Ok(result with { RefreshToken = null });
    }

    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        SetRefreshCookie(result.RefreshToken!);
        return Ok(result with { RefreshToken = null });
    }

    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var result = await Sender.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
        SetRefreshCookie(result.RefreshToken!);
        return Ok(result with { RefreshToken = null });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await Sender.Send(new LogoutCommand(), cancellationToken);
        ClearRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        await Sender.Send(new LogoutAllCommand(), cancellationToken);
        ClearRefreshCookie();
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

    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.AuthSensitive)]
    [HttpPost("send-verification")]
    public async Task<IActionResult> SendVerification(CancellationToken cancellationToken)
    {
        await Sender.Send(new SendVerificationEmailCommand(currentUser.UserId), cancellationToken);
        return NoContent();
    }

    [EnableRateLimiting(RateLimitPolicies.AuthSensitive)]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        SetRefreshCookie(result.RefreshToken!);
        return Ok(result with { RefreshToken = null });
    }

    [EnableRateLimiting(RateLimitPolicies.AuthSensitive)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }
}
