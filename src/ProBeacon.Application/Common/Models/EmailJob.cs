namespace ProBeacon.Application.Common.Models;

public record EmailJob(
    Guid TenantId,
    string To,
    string Subject,
    string HtmlBody
);
