using Application.Abstractions.Behaviors;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Registers the application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the mediator with pipeline behaviors and the FluentValidation validators.
    /// </summary>
    /// <param name="services">Service collection to register the services in.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(RequestLoggingPipelineBehavior<,>),
                typeof(ValidationPipelineBehavior<,>)
            ];
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<Modules.OpenData.Metadata.DatasetMetadataBuilder>();

        return services;
    }
}
