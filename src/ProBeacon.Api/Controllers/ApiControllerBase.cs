using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ProBeacon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>Name of the HttpOnly cookie that carries the opaque refresh token.</summary>
    protected const string RefreshCookieName = "pb_refresh";

    // Scope the cookie to the auth routes that read/clear it (refresh, logout). The browser
    // only attaches it to those paths, keeping it off every other request.
    private const string RefreshCookiePath = "/api/auth";
    private static readonly TimeSpan RefreshCookieLifetime = TimeSpan.FromDays(30);

    /// <summary>
    /// Issues the refresh token as an HttpOnly, SameSite=Strict cookie so it is never exposed
    /// to JavaScript. <c>Secure</c> follows the request scheme (off over plain-HTTP dev, on in
    /// production behind HTTPS).
    /// </summary>
    protected void SetRefreshCookie(string token) =>
        Response.Cookies.Append(
            RefreshCookieName,
            token,
            BuildRefreshCookieOptions(DateTimeOffset.UtcNow.Add(RefreshCookieLifetime)));

    protected void ClearRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, BuildRefreshCookieOptions(expires: null));

    private CookieOptions BuildRefreshCookieOptions(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = RefreshCookiePath,
        IsEssential = true,
        Expires = expires,
    };
}
