using Microsoft.AspNetCore.Http;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Infrastructure.Auth;

public class RequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public string UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

    public string IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    public string BaseUrl
    {
        get
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx is null) return string.Empty;
            return $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        }
    }
}
