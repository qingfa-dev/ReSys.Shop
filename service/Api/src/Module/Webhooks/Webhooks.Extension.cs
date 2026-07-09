using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Module.Webhooks.Persistence.Seeders;

using Shared.Operational.Persistence.Seeders;

namespace Module.Webhooks;

public static class WebhooksExtension
{
    public static WebApplicationBuilder AddWebhooksModule(this WebApplicationBuilder builder)
    {
        // TEMP: Dev-only seeder for order.placed webhook subscription
        builder.AddSeeder<WebhookSubscriptionSeeder>();

        return builder;
    }
}
