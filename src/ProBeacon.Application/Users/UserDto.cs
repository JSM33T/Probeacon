namespace ProBeacon.Application.Users;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    DateTime CreatedAt
);
