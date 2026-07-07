using System.Reflection;

using Microsoft.AspNetCore.Builder;

using Shared.Operational.Backgrounds;
using Shared.Operational.Http;
using Shared.Operational.Notifications;
using Shared.Operational.Persistence;
using Shared.Operational.Storages;
using Shared.Operational.Webhooks;

namespace Shared.Operational;

public static class OperationalExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddOperational(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        builder.AddStorage();
        builder.AddPersistence(additionalAssemblies);
        builder.AddNotifications();
        builder.AddBackgroundJobs();
        builder.AddHttpClients();
        builder.AddWebhooks();

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseOperational(this WebApplication app)
    {
        app.UseStorage();
        app.UseBackgroundJobs();

        return app;
    }

    #endregion
}
