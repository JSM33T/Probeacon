namespace ProBeacon.Application.Settings.Queries.GetSmtpSettings;

public record SmtpSettingsDto(
    string Host,
    int Port,
    string Username,
    bool HasPassword,
    string FromAddress,
    string FromName,
    bool EnableSsl,
    bool IsConfigured
);
