namespace ProBeacon.Application.Common.Exceptions;

public class WorkspaceExpiredException(string message = "This demo workspace has expired. Create a new demo workspace to continue.")
    : Exception(message);
