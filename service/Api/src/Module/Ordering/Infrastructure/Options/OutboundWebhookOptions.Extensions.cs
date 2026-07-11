using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Ordering.Infrastructure.Options;

public static class OutboundWebhookOptionsExtensions
{
    public static WebApplicationBuilder AddOutboundWebhooks(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<OutboundWebhookOptions>(
            builder.Configuration.GetSection(OutboundWebhookOptions.SectionName));
        return builder;
    }
}
