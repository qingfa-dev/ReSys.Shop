using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.Cors.Options;

namespace Shared.Security.Cors;

public static class CorsExtensions
{
    private const string WildcardOrigin = "*";

    public static WebApplicationBuilder AddSecurityCors(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidator<CorsSetting>, CorsSettingValidator>();

        builder.Services.AddOptions<CorsSetting>()
            .BindConfiguration(CorsSetting.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        CorsSetting options =
            builder.Configuration.GetSection(CorsSetting.SectionName)
                .Get<CorsSetting>() ?? new();

        builder.Services.AddCors(cors =>
        {
            cors.AddDefaultPolicy(policy =>
            {
                bool allowAnyOrigin =
                    options.Origins.Contains(WildcardOrigin);

                if (allowAnyOrigin)
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                    return;
                }

                if (options.Origins.Length > 0)
                {
                    policy.WithOrigins(options.Origins)
                          .SetIsOriginAllowed(origin =>
                          {
                              if (Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                                  && (uri.Host is "localhost" or "127.0.0.1"))
                                  return true;

                              return options.Origins.Contains(origin);
                          });
                }

                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader();

                if (options.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return builder;
    }

    public static WebApplication UseSecurityCors(this WebApplication app)
    {
        app.UseCors();
        return app;
    }
}