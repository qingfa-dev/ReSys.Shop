using System.Reflection;

using Microsoft.AspNetCore.Builder;

using Shared.Application.Endpoints;
using Shared.Application.Exceptions;
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
        builder.AddGlobalExceptionHandler();

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseApplication(this WebApplication app)
    {
        app.UseGlobalExceptionHandler();
        app.UseSystems();
        app.UseMediators();
        app.UseEndpoints();

        return app;
    }

    #endregion
}
