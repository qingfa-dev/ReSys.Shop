using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Module.Ordering.Infrastructure.Options;

namespace Module.Ordering.Features.Storefront.Cart.Checkout.Jobs;

public sealed class OrderPlacedDeliveryJob(
    IHttpClientFactory httpClientFactory,
    IOptions<OutboundWebhookOptions> options,
    ILogger<OrderPlacedDeliveryJob> logger)
{
    public async Task RunAsync(
        Guid orderId, string orderNumber, Guid userId, string email,
        decimal total, string currency, DateTimeOffset placedAtUtc,
        CancellationToken ct = default)
    {
        if (!options.Value.Enabled || options.Value.Urls.Count == 0)
        {
            logger.LogDebug("Outbound webhooks disabled or no URLs configured. Skipping.");
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            Event = "order.placed",
            OrderId = orderId,
            OrderNumber = orderNumber,
            UserId = userId,
            Email = email,
            Total = total,
            Currency = currency,
            PlacedAtUtc = placedAtUtc
        });

        using var client = httpClientFactory.CreateClient("OutboundWebhook");

        foreach (var url in options.Value.Urls)
        {
            try
            {
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content, ct);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Delivered order.placed to {Url}: {Status}", url, response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deliver order.placed to {Url}", url);
            }
        }
    }
}

public static class OrderPlacedDeliveryJobDefaults
{
    public const string JobId = "order-placed-delivery";
}
