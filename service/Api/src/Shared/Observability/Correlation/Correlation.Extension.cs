using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Observability.Correlation;

public static class CorrelationExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddObservabilityCorrelation(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseObservabilityCorrelation(this WebApplication app)
    {
        app.UseMiddleware<CorrelationMiddleware>();

        return app;
    }

    #endregion
}
