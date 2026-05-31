using Mediator;

namespace ProBeacon.Application.Settings.Queries.ExportSettings;

public record ExportSettingsQuery(bool IncludeSecrets) : IRequest<SettingsExport>;
