namespace ProBeacon.Application.Auth;

public record RefreshResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken
);
