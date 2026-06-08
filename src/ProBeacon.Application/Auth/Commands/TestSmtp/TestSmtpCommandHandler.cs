using Mediator;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.TestSmtp;

public class TestSmtpCommandHandler(ICurrentUser currentUser, IEmailSender emailSender)
    : IRequestHandler<TestSmtpCommand, TestSmtpResult>
{
    public async ValueTask<TestSmtpResult> Handle(TestSmtpCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Email.Contains('@'))
            return new TestSmtpResult(false, "Set a real email address in your profile before testing SMTP.");

        if (!await emailSender.IsConfiguredAsync(currentUser.TenantId, cancellationToken))
            return new TestSmtpResult(false, "SMTP is not configured. Save your SMTP settings before testing.");

        try
        {
            await emailSender.SendAsync(
                currentUser.TenantId,
                currentUser.Email,
                "ProBeacon SMTP test",
                """
                <!DOCTYPE html>
                <html>
                <body style="font-family:sans-serif;color:#111;max-width:520px;margin:40px auto;padding:0 20px;">
                  <h2 style="font-size:18px;">SMTP is working</h2>
                  <p>Your ProBeacon SMTP configuration is working correctly.</p>
                </body>
                </html>
                """,
                cancellationToken);

            return new TestSmtpResult(true, $"Test email sent to {currentUser.Email}.");
        }
        catch (Exception ex)
        {
            return new TestSmtpResult(false, ex.Message);
        }
    }
}
