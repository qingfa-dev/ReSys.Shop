using System.Reflection;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Mediators.Behaviours.Exceptions;
using Shared.Application.Mediators.Behaviours.Logging;
using Shared.Application.Mediators.Behaviours.Validation;

namespace Shared.Application.Mediators;

/// <summary>
/// Provides extension methods for configuring MediatR and associated behaviors in the dependency injection container.
/// </summary>
public static class Extensions
{
    #region Service Registration

    /// <summary>
    /// Registers MediatR services along with logging, validation, and exception mapping behaviors.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration for retrieving settings.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for validators and handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddMediators(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        #region MediatR Configuration

        // Initialize: Configure MediatR with pipeline behaviors
        builder.Services.AddMediatR(cfg =>
        {
            // Register: Handlers from the current building block assembly
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Register: Handlers from additional assemblies (e.g., Module)
            foreach (var assembly in additionalAssemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }

            // Add: Pipeline behaviors in priority order (Outer to Inner)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionMappingBehavior<,>));
        });

        #endregion

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    /// <summary>
    /// Initializes MediatR services for the application, ensuring services are warmed up.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication UseMediators(this WebApplication app)
    {
        #region Service Warming

        // Resolve: Warm up the mediator service to ensure readiness
        app.Services.GetRequiredService<IMediator>();

        #endregion

        return app;
    }

    #endregion
}
