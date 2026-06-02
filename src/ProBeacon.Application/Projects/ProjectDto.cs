namespace ProBeacon.Application.Projects;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    string AccessRole,
    int MemberCount
);
