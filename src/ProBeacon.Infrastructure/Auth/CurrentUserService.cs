using Microsoft.AspNetCore.Http;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid TenantId => GetGuidClaim("tenant_id");
    public Guid UserId => GetGuidClaim("sub");
    public Guid SessionId => GetGuidClaim("session_id");
    public string Email => GetStringClaim("email");

    private Guid GetGuidClaim(string type)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException($"Missing or invalid claim: {type}");
    }

    private string GetStringClaim(string type)
        => httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value
            ?? throw new UnauthorizedAccessException($"Missing claim: {type}");
}
