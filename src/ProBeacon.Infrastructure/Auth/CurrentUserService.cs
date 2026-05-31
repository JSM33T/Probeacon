using Microsoft.AspNetCore.Http;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid TenantId => GetClaim("tenant_id");
    public Guid UserId => GetClaim("sub");
    public Guid SessionId => GetClaim("session_id");

    private Guid GetClaim(string type)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException($"Missing or invalid claim: {type}");
    }
}
