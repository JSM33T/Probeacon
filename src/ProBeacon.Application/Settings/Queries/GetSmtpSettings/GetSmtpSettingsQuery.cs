using Mediator;

namespace ProBeacon.Application.Settings.Queries.GetSmtpSettings;

public record GetSmtpSettingsQuery : IRequest<SmtpSettingsDto>;
