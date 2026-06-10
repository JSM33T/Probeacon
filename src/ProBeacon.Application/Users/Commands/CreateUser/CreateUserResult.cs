using ProBeacon.Application.Users;

namespace ProBeacon.Application.Users.Commands.CreateUser;

public record CreateUserResult(
    UserDto User,
    // Set only when SMTP is unconfigured: the admin must hand this set-password link over
    // manually. Null when an invite email was sent.
    string? InviteLink = null
);
