using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.Headers.Options;

namespace Shared.Security.Headers;

public static class SecurityHeadersExtensions
{
    public static WebApplicationBuilder AddSecurityHeaders(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidator<SecurityHeadersSetting>, SecurityHeadersSettingValidator>();

        builder.Services.AddOptions<SecurityHeadersSetting>()
            .BindConfiguration(SecurityHeadersSetting.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        return builder;
    }

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}
