using Mediator;
using ProBeacon.Application.Settings.Queries.GetLockoutSettings;

namespace ProBeacon.Application.Settings.Commands.UpsertLockoutSettings;

public record UpsertLockoutSettingsCommand(bool Enabled, int MaxAttempts, int BaseMinutes, int MaxMinutes)
    : IRequest<LockoutSettingsDto>;
