namespace ProBeacon.Application.Auth.Queries.GetSessions;

public record SessionDto(
    Guid Id,
    string UserAgent,
    string IpAddress,
    DateTime CreatedAt,
    DateTime LastActiveAt,
    bool IsCurrentSession
);
