namespace ProBeacon.Application.Users.Queries.GetProfile;

public record ProfileDto(Guid Id, string Email, string DisplayName, string Role);
