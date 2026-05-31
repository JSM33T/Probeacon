using System.Security.Cryptography;
using System.Text;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(IApplicationDbContext db)
    : ICommandHandler<VerifyEmailCommand>
{
    public async ValueTask<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        string tokenHash;
        try
        {
            tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)))
                .ToLowerInvariant();
        }
        catch
        {
            throw new InvalidOperationException("Invalid verification token.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.EmailVerificationTokenHash == tokenHash &&
            u.EmailVerificationTokenExpiresAt > DateTime.UtcNow &&
            u.EmailVerifiedAt == null,
            cancellationToken);

        if (user is null)
            throw new InvalidOperationException("Invalid or expired verification token.");

        user.MarkEmailVerified();
        await db.SaveChangesAsync(cancellationToken);

        return default;
    }
}
