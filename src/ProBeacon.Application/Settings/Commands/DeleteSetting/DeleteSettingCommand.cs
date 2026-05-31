using Mediator;

namespace ProBeacon.Application.Settings.Commands.DeleteSetting;

public record DeleteSettingCommand(string Key) : ICommand;
