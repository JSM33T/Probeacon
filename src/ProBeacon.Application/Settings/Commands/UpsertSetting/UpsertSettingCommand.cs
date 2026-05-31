using Mediator;

namespace ProBeacon.Application.Settings.Commands.UpsertSetting;

public record UpsertSettingCommand(string Key, string Value) : IRequest<SettingDto>;
