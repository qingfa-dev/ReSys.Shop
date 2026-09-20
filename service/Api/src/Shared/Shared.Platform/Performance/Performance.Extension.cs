using Microsoft.AspNetCore.Builder;

using Shared.Performance.Caching;

namespace Shared.Performance;

public static class PerformanceExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddPerformance(
        this WebApplicationBuilder builder)
    {
        builder.AddCaching();

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UsePerformance(this WebApplication app)
    {
        return app;
    }

    #endregion
}
