using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shared.Application.Systems.SystemDateTimes;
using Shared.Application.Systems.SystemInfos;

namespace Shared.Application.Systems;

public static class SystemsExtensions
{
    public static WebApplicationBuilder AddSystems(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<ISystemDateTime, SystemDateTime>();
        builder.Services.TryAddSingleton<ISystemInfo, SystemInfo>();

        return builder;
    }

    public static WebApplication UseSystems(this WebApplication app)
    {
        app.Services.GetRequiredService<ISystemDateTime>();

        return app;
    }
}
