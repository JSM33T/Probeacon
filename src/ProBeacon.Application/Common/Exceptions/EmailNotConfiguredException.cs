namespace ProBeacon.Application.Common.Exceptions;

public class EmailNotConfiguredException(
    string message = "Email delivery (SMTP) is not configured. Configure SMTP under Settings before sending emails.")
    : Exception(message);
