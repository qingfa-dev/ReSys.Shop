using System.Reflection;

using Carter;

using Microsoft.AspNetCore.Builder;

namespace Shared.Application.Endpoints;

public static class EndpointExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddEndpoints(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        List<Assembly> assembliesToScan = [..additionalAssemblies];

        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is not null && !assembliesToScan.Contains(entryAssembly))
        {
            assembliesToScan.Add(entryAssembly);
        }

        Assembly sharedAssembly = typeof(EndpointExtension).Assembly;
        if (!assembliesToScan.Contains(sharedAssembly))
        {
            assembliesToScan.Add(sharedAssembly);
        }

        builder.Services.AddCarter(
            new DependencyContextAssemblyCatalog(),
            configurator =>
            {
                foreach (Assembly assembly in assembliesToScan)
                {
                    Type[] moduleTypes = assembly.GetTypes()
                        .Where(t => !t.IsAbstract && typeof(ICarterModule).IsAssignableFrom(t))
                        .ToArray();

                    if (moduleTypes.Length > 0)
                    {
                        configurator.WithModules(moduleTypes);
                    }
                }
            });

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseEndpoints(this WebApplication app)
    {
        app.MapCarter();

        return app;
    }

    #endregion
}
