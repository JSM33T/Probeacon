using ProBeacon.Application.Common.Models;

namespace ProBeacon.Application.Common.Interfaces;

public interface IEmailJobPublisher
{
    Task PublishAsync(EmailJob job, CancellationToken cancellationToken = default);
}
