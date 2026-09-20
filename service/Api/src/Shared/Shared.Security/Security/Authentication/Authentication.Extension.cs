using Microsoft.AspNetCore.Builder;

using Shared.Security.Authentication.Contexts;
using Shared.Security.Authentication.External;
using Shared.Security.Authentication.Guest;
using Shared.Security.Authentication.Tokens;

namespace Shared.Security.Authentication;

/// <summary>
/// Provides extension methods for configuring authentication services in the dependency injection container.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers all authentication services including JWT bearer authentication,
    /// token services, and current-user context resolution.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration for retrieving settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddApplicationAuthentication(this WebApplicationBuilder builder)
    {
        // Register: JWT authentication, token services, and security protections
        builder.AddTokenAuthentication();

        // Register: Current user context resolution from HTTP context
        builder.AddAuthenticationContext();

        // Add: External login providers (Google, etc.)
        builder.AddExternalAuthentication();

        // Add: Guest session for anonymous session tracking
        builder.AddGuestSession();

        return builder;
    }

    /// <summary>
    /// Enables authentication and authorization middleware for the application.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication UseApplicationAuthentication(this WebApplication app)
    {
        app.UseGuestSession();
        app.UseAuthentication();
        return app;
    }


}
