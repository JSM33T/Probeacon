using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProBeacon.Api.Authorization;
using ProBeacon.Application.Projects.Commands.CreateProject;
using ProBeacon.Application.Projects.Commands.DeleteProject;
using ProBeacon.Application.Projects.Commands.RemoveProjectMember;
using ProBeacon.Application.Projects.Commands.UpdateProject;
using ProBeacon.Application.Projects.Commands.UpsertProjectMember;
using ProBeacon.Application.Projects.Queries.GetProject;
using ProBeacon.Application.Projects.Queries.GetProjectMembers;
using ProBeacon.Application.Projects.Queries.GetProjects;

namespace ProBeacon.Api.Controllers;

[Authorize]
public class ProjectsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProjects(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetProjectsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Create(CreateProjectCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetProject(Guid projectId, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetProjectQuery(projectId), cancellationToken));

    [HttpPatch("{projectId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Update(
        Guid projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
        => Ok(await Sender.Send(
            new UpdateProjectCommand(projectId, request.Name, request.Description),
            cancellationToken));

    [HttpDelete("{projectId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteProjectCommand(projectId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{projectId:guid}/members")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> GetMembers(Guid projectId, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetProjectMembersQuery(projectId), cancellationToken));

    [HttpPut("{projectId:guid}/members/{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> UpsertMember(
        Guid projectId,
        Guid userId,
        UpsertProjectMemberRequest request,
        CancellationToken cancellationToken)
        => Ok(await Sender.Send(
            new UpsertProjectMemberCommand(projectId, userId, request.Role),
            cancellationToken));

    [HttpDelete("{projectId:guid}/members/{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> RemoveMember(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await Sender.Send(new RemoveProjectMemberCommand(projectId, userId), cancellationToken);
        return NoContent();
    }
}

public record UpdateProjectRequest(string Name, string? Description);

public record UpsertProjectMemberRequest(string Role);
