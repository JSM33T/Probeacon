using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Users.Queries.GetProfile;

namespace ProBeacon.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher)
    : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    public async ValueTask<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.UpdateDisplayName(request.DisplayName.Trim());

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailTaken = await db.Users
                .AnyAsync(u => u.Email == request.Email.ToLowerInvariant() && u.Id != user.Id, cancellationToken);

            if (emailTaken)
                throw new InvalidOperationException("Email is already in use.");

            user.UpdateEmail(request.Email);
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                throw new InvalidOperationException("Current password is required to set a new password.");

            if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");

            user.UpdatePasswordHash(passwordHasher.Hash(request.NewPassword));
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ProfileDto(user.Id, user.Email, user.DisplayName, user.Role.ToString());
    }
}
