using System.Reflection;

using Microsoft.AspNetCore.Builder;

using Shared.Governance.OpenApi;
using Shared.Governance.Validation;

namespace Shared.Governance;

/// <summary>
/// Provides extension methods for configuring cross-cutting governance services.
/// </summary>
public static class GovernanceExtension
{
    #region Service Registration

    /// <summary>
    /// Registers all governance services including FluentValidation validators.
    /// </summary>
    /// <param name="builder">The web application builder to add services to.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for validators.</param>
    /// <returns>The web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddGovernance(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        // Subscribe: Register OpenAPI documentation services
        builder.AddOpenApiDocumentation();

        // Subscribe: Register governance validation services
        builder.AddFluentValidation(additionalAssemblies);

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    /// <summary>
    /// Enables governance middleware in the application pipeline including OpenAPI and Scalar UI.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication UseGovernance(this WebApplication app)
    {
        // Contract: pre=app!=null
        ArgumentNullException.ThrowIfNull(app);

        // Map: OpenAPI endpoints and Scalar interactive UI
        app.UseOpenApiDocumentation();

        return app;
    }

    #endregion
}