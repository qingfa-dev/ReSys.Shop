using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.External.Providers.Facebook;
using Shared.Security.Authentication.External.Providers.Facebook.Options;
using Shared.Security.Authentication.External.Providers.Google;
using Shared.Security.Authentication.External.Providers.Google.Options;
using Shared.Security.Authentication.External.Providers.Microsoft;
using Shared.Security.Authentication.External.Providers.Microsoft.Options;
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

        // Facebook
        builder.Services.AddOptions<FacebookOptions>()
            .BindConfiguration(FacebookOptions.SectionName)
            .ValidateFluentValidation();

        builder.Services.AddHttpClient("FacebookGraph", c => c.BaseAddress = new Uri("https://graph.facebook.com/"));

        builder.Services.AddScoped<IFacebookTokenValidator, FacebookTokenValidator>();

        if (builder.Configuration.GetValue<bool>("Authentication:Facebook:Enabled"))
        {
            builder.Services.AddScoped<IExternalLoginProvider, FacebookExternalProvider>();
        }

        // Microsoft
        builder.Services.AddOptions<MicrosoftOptions>()
            .BindConfiguration(MicrosoftOptions.SectionName)
            .ValidateFluentValidation();

        builder.Services.AddHttpClient("MicrosoftGraph", c => c.BaseAddress = new Uri("https://graph.microsoft.com/"));

        builder.Services.AddScoped<IMicrosoftTokenValidator, MicrosoftTokenValidator>();

        if (builder.Configuration.GetValue<bool>("Authentication:Microsoft:Enabled"))
        {
            builder.Services.AddScoped<IExternalLoginProvider, MicrosoftExternalProvider>();
        }

        // Register: ExternalProviderDiscoveryService as scoped (its IExternalLoginProvider dependencies are scoped)
        builder.Services.AddScoped<ExternalProviderRegistry>();

        return builder;
    }
}
