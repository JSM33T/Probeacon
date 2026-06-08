namespace ProBeacon.Application.Projects.Queries.GetAssignableUsers;

public record AssignableUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive
);
