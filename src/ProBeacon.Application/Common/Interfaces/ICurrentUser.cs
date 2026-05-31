namespace ProBeacon.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid TenantId { get; }
    Guid UserId { get; }
    Guid SessionId { get; }
    string Email { get; }
}
