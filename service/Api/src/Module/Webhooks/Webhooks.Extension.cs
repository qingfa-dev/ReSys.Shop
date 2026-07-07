using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Webhooks;

public static class WebhooksExtension
{
    public static WebApplicationBuilder AddWebhooksModule(this WebApplicationBuilder builder)
    {
        return builder;
    }
}
