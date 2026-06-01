namespace ProBeacon.Application.Setup.Queries.GetSetupStatus;

public record SetupStatusResult(
    bool Configured,
    string DeploymentMode,
    int DemoWorkspaceLifetimeHours
);
