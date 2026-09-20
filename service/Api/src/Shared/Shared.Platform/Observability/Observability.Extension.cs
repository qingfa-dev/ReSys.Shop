using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Application.Extensions.Validations;
using Shared.Observability.Correlation;
using Shared.Observability.HealthChecks;
using Shared.Observability.Logging;

namespace Shared.Observability;

public static class ObservabilityExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        // Register validator
        builder.Services.AddSingleton<IValidator<ObservabilitySetting>, ObservabilitySettingValidator>();

        // Register options + FluentValidation
        builder.Services
            .AddOptions<ObservabilitySetting>()
            .BindConfiguration(ObservabilitySetting.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        // Bridge: make ObservabilitySetting directly injectable for CorrelationMiddleware and health checks
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<ObservabilitySetting>>().Value);

        builder.AddObservabilityCorrelation();
        builder.AddObservabilityLogging();
        builder.AddObservabilityHealthChecks();

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseObservability(this WebApplication app)
    {
        app.UseObservabilityCorrelation();
        app.UseObservabilityLogging();
        app.UseObservabilityHealthChecks();

        return app;
    }

    #endregion
}
