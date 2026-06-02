using ProBeacon.Application.Users;

namespace ProBeacon.Application.Users.Commands.CreateUser;

public record CreateUserResult(
    UserDto User,
    string TemporaryPassword
);
