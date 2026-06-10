using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Services;
using ProBeacon.Application.Users.Commands.SendPasswordSetupEmail;
using ProBeacon.Domain.Entities;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,
    IPasswordSetupMailer mailer)
    : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public async ValueTask<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var role = Enum.Parse<UserRole>(request.Role, true);
        var emailTaken = await db.Users.AnyAsync(user => user.Email == email, cancellationToken);
        if (emailTaken)
            throw new ConflictException("Email is already in use.");

        // The account starts with an undisclosed random password — it can't be used until the
        // user sets their own password via the invite link below.
        var unusableSecret = TemporaryPasswordGenerator.Generate();
        var user = User.Create(
            currentUser.TenantId,
            email,
            request.DisplayName.Trim(),
            passwordHasher.Hash(unusableSecret),
            role);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        // Always issue the invite link. If SMTP is configured, email it; otherwise hand the link
        // back so the admin can deliver it manually — self-hosted without email still works.
        var link = await mailer.IssueLinkAsync(user, PasswordSetupKind.Invite, cancellationToken);
        var smtpConfigured = await emailSender.IsConfiguredAsync(currentUser.TenantId, cancellationToken);
        if (smtpConfigured)
            await mailer.SendAsync(user, PasswordSetupKind.Invite, link, cancellationToken);

        return new CreateUserResult(
            new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Role.ToString(),
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt),
            InviteLink: smtpConfigured ? null : link);
    }
}
