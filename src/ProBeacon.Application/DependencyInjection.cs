using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using ProBeacon.Application.Common.Behaviors;
using System.Reflection;

namespace ProBeacon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    private static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select(type => new
            {
                Implementation = type,
                Services = type.GetInterfaces()
                    .Where(@interface => @interface.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .ToArray()
            })
            .Where(type => type.Services.Length > 0);

        foreach (var validatorType in validatorTypes)
            foreach (var serviceType in validatorType.Services)
                services.AddScoped(serviceType, validatorType.Implementation);

        return services;
    }
}
