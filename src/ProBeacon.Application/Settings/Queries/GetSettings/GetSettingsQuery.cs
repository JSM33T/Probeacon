using MediatR;

namespace ProBeacon.Application.Settings.Queries.GetSettings;

public record GetSettingsQuery : IRequest<List<SettingDto>>;
