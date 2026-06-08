using Mediator;

namespace ProBeacon.Application.Projects.Queries.GetAssignableUsers;

/// <summary>
/// Active users in the tenant that a project Manager (or global Admin) can add to a project.
/// Scoped to the project so a Manager who isn't a global Admin can still pick people to assign.
/// </summary>
public record GetAssignableUsersQuery(Guid ProjectId) : IRequest<IReadOnlyList<AssignableUserDto>>;
