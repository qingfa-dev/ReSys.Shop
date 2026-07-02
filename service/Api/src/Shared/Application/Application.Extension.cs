using System.Reflection;

using Microsoft.AspNetCore.Builder;

using Shared.Application.Endpoints;
using Shared.Application.Mediators;
using Shared.Application.Systems;

namespace Shared.Application;

public static class ApplicationExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddApplication(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        builder.AddSystems();
        builder.AddMediators(additionalAssemblies);
        builder.AddEndpoints(additionalAssemblies);

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseApplication(this WebApplication app)
    {
        app.UseSystems();
        app.UseMediators();
        app.UseEndpoints();

        return app;
    }

    #endregion
}
