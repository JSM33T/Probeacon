namespace ProBeacon.Application.Common.Options;

public class AppOptions
{
    /// <summary>
    /// Public base URL of the frontend app (no trailing slash), used to build
    /// links sent in emails (e.g. email verification). Falls back to the API
    /// request base URL when not configured.
    /// </summary>
    public string FrontendUrl { get; set; } = string.Empty;

    public DeploymentMode DeploymentMode { get; set; } = DeploymentMode.SelfHosted;

    public bool IsSelfHosted => DeploymentMode == DeploymentMode.SelfHosted;

    public bool IsOnlineDemo => DeploymentMode == DeploymentMode.OnlineDemo;
}
