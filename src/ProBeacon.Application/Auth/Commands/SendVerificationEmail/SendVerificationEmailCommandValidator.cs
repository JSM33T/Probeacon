using FluentValidation;

namespace ProBeacon.Application.Auth.Commands.SendVerificationEmail;

public class SendVerificationEmailCommandValidator : AbstractValidator<SendVerificationEmailCommand>
{
    public SendVerificationEmailCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
