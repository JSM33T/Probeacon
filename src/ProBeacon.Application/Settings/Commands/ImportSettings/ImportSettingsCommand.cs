using Mediator;

namespace ProBeacon.Application.Settings.Commands.ImportSettings;

public record ImportSettingsCommand(
    IReadOnlyDictionary<string, string> Settings,
    bool Replace
) : IRequest<ImportSettingsResult>;
