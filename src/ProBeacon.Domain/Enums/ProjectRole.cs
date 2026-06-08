namespace ProBeacon.Domain.Enums;

/// <summary>
/// A user's access level on a single project. Independent of the global <see cref="UserRole"/>
/// (a global Member can be a Manager of one project). Ordered least → most privileged so
/// comparisons (e.g. <c>Role &gt;= ProjectRole.Editor</c>) work.
/// </summary>
public enum ProjectRole
{
    /// <summary>Read-only access.</summary>
    Viewer = 0,

    /// <summary>Read + edit project properties (and, later, probes).</summary>
    Editor = 1,

    /// <summary>Editor + manage this project's members and their roles. Cannot delete the project.</summary>
    Manager = 2
}
