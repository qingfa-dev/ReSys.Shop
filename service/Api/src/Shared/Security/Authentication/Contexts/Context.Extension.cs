using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Security.Authentication.Contexts;

public static class Extensions
{
    /// <summary>
    /// Registers current user context services for resolving the authenticated user from HTTP context.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddAuthenticationContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        return builder;
    }
}