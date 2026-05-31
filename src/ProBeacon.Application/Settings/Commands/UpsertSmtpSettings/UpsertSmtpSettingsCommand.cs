using Mediator;
using ProBeacon.Application.Settings.Queries.GetSmtpSettings;

namespace ProBeacon.Application.Settings.Commands.UpsertSmtpSettings;

public record UpsertSmtpSettingsCommand(
    string Host,
    int Port,
    string Username,
    string? Password,
    string FromAddress,
    string FromName,
    bool EnableSsl
) : IRequest<SmtpSettingsDto>;
