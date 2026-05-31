namespace ProBeacon.Application.Auth;

public record LoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    Guid SessionId,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role
);
