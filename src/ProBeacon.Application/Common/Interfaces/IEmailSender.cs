namespace ProBeacon.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(Guid tenantId, string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether SMTP is configured for the given tenant (falling back to env-level
    /// config when the tenant has no SMTP settings). Pass <see cref="Guid.Empty"/> to check
    /// the env-level fallback only (e.g. before a tenant exists during provisioning).
    /// </summary>
    Task<bool> IsConfiguredAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
