using FluentValidation;
using Mediator;
using ProBeacon.Application.Common.Behaviors;
using Xunit;

namespace ProBeacon.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_CallsNextHandler()
    {
        var behavior = new ValidationBehavior<TestCommand, string>([
            new RequiredNameValidator()
        ]);
        var called = false;

        var result = await behavior.Handle(
            new TestCommand("valid"),
            (_, _) =>
            {
                called = true;
                return ValueTask.FromResult("ok");
            },
            CancellationToken.None);

        Assert.True(called);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_WhenCommandIsInvalid_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestCommand, string>([
            new RequiredNameValidator()
        ]);
        var called = false;

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await behavior.Handle(
                new TestCommand(""),
                (_, _) =>
                {
                    called = true;
                    return ValueTask.FromResult("ok");
                },
                CancellationToken.None));

        Assert.False(called);
        Assert.Contains(exception.Errors, error => error.PropertyName == nameof(TestCommand.Name));
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsFail_AggregatesFailures()
    {
        var behavior = new ValidationBehavior<TestCommand, string>([
            new FirstFailingValidator(),
            new SecondFailingValidator()
        ]);

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await behavior.Handle(
                new TestCommand(""),
                (_, _) => ValueTask.FromResult("ok"),
                CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.ErrorMessage == "First failure.");
        Assert.Contains(exception.Errors, error => error.ErrorMessage == "Second failure.");
    }

    private sealed record TestCommand(string Name) : IRequest<string>;

    private sealed class RequiredNameValidator : AbstractValidator<TestCommand>
    {
        public RequiredNameValidator()
        {
            RuleFor(command => command.Name).NotEmpty();
        }
    }

    private sealed class FirstFailingValidator : AbstractValidator<TestCommand>
    {
        public FirstFailingValidator()
        {
            RuleFor(command => command.Name)
                .Must(_ => false)
                .WithMessage("First failure.");
        }
    }

    private sealed class SecondFailingValidator : AbstractValidator<TestCommand>
    {
        public SecondFailingValidator()
        {
            RuleFor(command => command.Name)
                .Must(_ => false)
                .WithMessage("Second failure.");
        }
    }
}
