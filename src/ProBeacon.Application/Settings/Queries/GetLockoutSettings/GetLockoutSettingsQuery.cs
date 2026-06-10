using Mediator;

namespace ProBeacon.Application.Settings.Queries.GetLockoutSettings;

public record GetLockoutSettingsQuery : IRequest<LockoutSettingsDto>;
