using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.RevokeSession;

public class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(command => command.SessionId)
            .NotEmpty();
    }
}
