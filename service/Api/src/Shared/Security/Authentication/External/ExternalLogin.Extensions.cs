using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.External.Providers.Google;
using Shared.Security.Authentication.External.Providers.Google.Options;
using Shared.Security.Authentication.External.Services;

namespace Shared.Security.Authentication.External;

/// <summary>
/// Extension methods for registering external authentication services.
/// </summary>
public static class ExternalLoginExtensions
{
    /// <summary>
    /// Registers external login providers and binds configuration options.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static WebApplicationBuilder AddExternalAuthentication(this WebApplicationBuilder builder)
    {
        // Configure: Bind GoogleExternalLoginOptions from Authentication:Google section
        builder.Services.AddOptions<GoogleOptions>()
            .BindConfiguration(GoogleOptions.SectionName)
            .ValidateFluentValidation();

        // Register: IGoogleTokenValidator for mockable Google token validation
        builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        // Register: IExternalLoginProvider as scoped service — resolves to GoogleExternalLoginProvider
        builder.Services.AddScoped<IExternalLoginProvider, GoogleExternalProvider>();

        // Register: ExternalProviderDiscoveryService as scoped (its IExternalLoginProvider dependencies are scoped)
        builder.Services.AddScoped<ExternalProviderRegistry>();

        return builder;
    }
}
