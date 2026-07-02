using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shared.Application.Systems.SystemDateTimes;
using Shared.Application.Systems.SystemInfos;

namespace Shared.Application.Systems;

/// <summary>
/// Provides extension methods for registering core system services such as date-time, system info, and generators.
/// </summary>
public static class SystemsExtensions
{
    #region Service Registration

    /// <summary>
    /// Registers all core system building block services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddSystems(this WebApplicationBuilder builder)
    {
        // Add: System clock abstraction for testable time logic
        builder.Services.TryAddSingleton<ISystemDateTime, SystemDateTime>();

        // Add: System metadata and runtime information provider
        builder.Services.TryAddSingleton<ISystemInfo, SystemInfo>();

        return builder;
    }
    #endregion
     #region Pipeline Configuration

    /// <summary>
    /// Initializes Systems services for the application, ensuring services are warmed up.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication UseSystems(this WebApplication app)
    {
        #region Service Warming

        // Resolve: Warm up the system service to ensure readiness
        app.Services.GetRequiredService<ISystemDateTime>();

        #endregion

        return app;
    }

    #endregion
}
