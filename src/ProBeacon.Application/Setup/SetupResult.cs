namespace ProBeacon.Application.Setup;

public record SetupResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    Guid SessionId,
    Guid UserId,
    string Email,
    string DisplayName
);
