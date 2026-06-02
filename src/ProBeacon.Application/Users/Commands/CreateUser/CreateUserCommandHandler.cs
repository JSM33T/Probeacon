using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Services;
using ProBeacon.Domain.Entities;
using ProBeacon.Domain.Enums;

namespace ProBeacon.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public async ValueTask<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var role = Enum.Parse<UserRole>(request.Role, true);
        var emailTaken = await db.Users.AnyAsync(user => user.Email == email, cancellationToken);
        if (emailTaken)
            throw new ConflictException("Email is already in use.");

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        var user = User.Create(
            currentUser.TenantId,
            email,
            request.DisplayName.Trim(),
            passwordHasher.Hash(temporaryPassword),
            role);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateUserResult(
            new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Role.ToString(),
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt),
            temporaryPassword);
    }

}
