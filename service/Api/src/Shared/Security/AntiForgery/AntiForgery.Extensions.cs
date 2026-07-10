using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.AntiForgery.Options;

namespace Shared.Security.AntiForgery;

public static class AntiForgeryExtensions
{
    public static WebApplicationBuilder AddAntiForgery(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidator<AntiForgerySetting>, AntiForgerySettingValidator>();
        builder.Services.AddOptions<AntiForgerySetting>()
            .BindConfiguration(AntiForgerySetting.SectionName)
            .ValidateFluentValidation();

        // Compute: Materialize validated options for middleware configuration
        AntiForgerySetting antiForgeryOptions =
            builder.Configuration.GetSection(AntiForgerySetting.SectionName).Get<AntiForgerySetting>() ??
            new AntiForgerySetting();

        // Assign: Pass validated values to ASP.NET Core antiforgery middleware
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = antiForgeryOptions.HeaderName;

            options.Cookie.Name = antiForgeryOptions.CookieName;
            options.Cookie.SameSite = Enum.Parse<SameSiteMode>(antiForgeryOptions.CookieSameSite, ignoreCase: true);
            options.Cookie.SecurePolicy = Enum.Parse<CookieSecurePolicy>(antiForgeryOptions.CookieSecurePolicy, ignoreCase: true);
            options.Cookie.HttpOnly = antiForgeryOptions.CookieHttpOnly;

            if (antiForgeryOptions.CookieMaxAgeMinutes.HasValue)
            {
                options.Cookie.MaxAge = TimeSpan.FromMinutes(antiForgeryOptions.CookieMaxAgeMinutes.Value);
            }
        });

        return builder;
    }

    public static WebApplication UseAntiForgery(this WebApplication app)
    {
        return app;
    }
}
