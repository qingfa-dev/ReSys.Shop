using System.IdentityModel.Tokens.Jwt;
using System.Text;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Shared.Application.Extensions.Validations;
using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;

namespace Shared.Security.Authentication.Tokens;

/// <summary>
/// Provides extension methods for configuring JWT authentication, token services, and security protections.
/// </summary>
public static class TokensExtensions
{
    /// <summary>
    /// Configures JWT bearer authentication with token validation parameters and registers all token services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration for retrieving JWT settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddTokenAuthentication(this WebApplicationBuilder builder)
    {
        // Clear: Default claim type mappings to preserve original claim names
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // Register: JwtSettingsValidator bound to the host environment so the dev secret literal is
        // refused in any non-Development environment. The explicit singleton overrides the
        // auto-registered Scoped registration from AddValidatorsFromAssembly.
        builder.Services.AddSingleton<IValidator<JwtSettings>>(sp =>
            new JwtSettingsValidator(sp.GetRequiredService<IHostEnvironment>()));

        // Bind: Load and validate JWT settings with FluentValidation
        builder.Services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        // Register: Refresh token persistence and protection services
        builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        builder.Services.AddScoped<ITokenTheftDetector, TokenTheftDetector>();
        builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

        // Register: Token generation services
        builder.Services.AddSingleton<IAccessTokenService, AccessTokenService>();
        // Scoped to consume IRefreshTokenStore, ITokenTheftDetector, and ICurrentUser
        builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Configure: JWT bearer as the default authentication scheme
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        // Configure: Token validation parameters from bound JWT settings
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
            {
                JwtSettings opts = jwtSettings.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = opts.Issuer,
                    ValidAudience = opts.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Secret)),
                    ClockSkew = TimeSpan.Zero,
                    ValidAlgorithms = [opts.Algorithm]
                };
            });

        return builder;
    }
}
