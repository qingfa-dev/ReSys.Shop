using System.Reflection;

using FluentValidation;

using Microsoft.AspNetCore.Builder;

namespace Shared.Governance.Validation;

/// <summary>
/// Provides extension methods for registering FluentValidation validators in the DI container.
/// </summary>
public static class ValidationExtensions
{
    #region Service Registration

    /// <summary>
    /// Registers FluentValidation validators from the entry, current (Shared), and any additional assemblies.
    /// </summary>
    /// <param name="builder">The web application builder to add services to.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for validators.</param>
    /// <returns>The web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddFluentValidation(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        // Subscribe: Register validators from entry, current (Shared), and additional assemblies
        builder.Services.AddValidatorsFromAssembly(Assembly.GetEntryAssembly());
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (Assembly assembly in additionalAssemblies)
        {
            builder.Services.AddValidatorsFromAssembly(assembly);
        }

        return builder;
    }

    #endregion
}
