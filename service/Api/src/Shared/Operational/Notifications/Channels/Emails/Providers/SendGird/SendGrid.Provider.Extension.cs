using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shared.Operational.Notifications.Providers;

namespace Shared.Operational.Notifications.Channels.Emails.Providers.SendGird;

/// <summary>Registers the SendGrid notification provider with the DI container.</summary>
public static class SendGridProviderExtensions
{
    /// <summary>Adds the SendGrid email provider as a scoped INotificationProvider service.</summary>
    public static WebApplicationBuilder AddEmailSendGridProvider(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<INotificationProvider, SendGridProvider>();
        return builder;
    }
}
