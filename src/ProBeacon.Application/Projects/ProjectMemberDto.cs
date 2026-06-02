namespace ProBeacon.Application.Projects;

public record ProjectMemberDto(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    string Role,
    DateTime AssignedAt,
    Guid AssignedByUserId
);
