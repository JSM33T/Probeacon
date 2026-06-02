using Mediator;

namespace ProBeacon.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid ProjectId) : ICommand;
