using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Webhooks.Domain;

namespace Shared.Operational.Webhooks.Services;

public sealed class WebhookDispatcher : IWebhookDispatcher
{
    private const string SignatureHeader = "X-Webhook-Signature";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebhookSigner _signer;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<WebhookDispatcher> _logger;

    public WebhookDispatcher(
        IHttpClientFactory httpClientFactory,
        IWebhookSigner signer,
        IApplicationDbContext dbContext,
        ILogger<WebhookDispatcher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _signer = signer;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result> PublishAsync(string eventName, object payload, CancellationToken ct = default)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        var subscriptions = await _dbContext.Set<WebhookSubscription>()
            .Where(s => s.Event == eventName && s.Active)
            .ToListAsync(ct);

        foreach (var sub in subscriptions)
        {
            var deliveryResult = WebhookDeliveryMethod.Create(
                subscriptionId: sub.Id,
                @event: eventName,
                payloadJson: payloadJson);
            if (deliveryResult.IsSuccess)
                _dbContext.Set<WebhookDelivery>().Add(deliveryResult.Value);
        }
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<WebhookDelivery>> DeliverAsync(
        WebhookSubscription subscription, WebhookDelivery delivery, CancellationToken ct = default)
    {
        try
        {
            var signature = _signer.Sign(delivery.PayloadJson, subscription.SecretHash);
            using var http = _httpClientFactory.CreateClient("WebhookHttp");
            using var req = new HttpRequestMessage(HttpMethod.Post, subscription.Url)
            {
                Content = new StringContent(delivery.PayloadJson, Encoding.UTF8, "application/json")
            };
            req.Headers.TryAddWithoutValidation(SignatureHeader, $"sha256={signature}");
            req.Headers.TryAddWithoutValidation("X-Webhook-Event", delivery.Event);
            req.Headers.TryAddWithoutValidation("X-Webhook-Delivery-Id", delivery.Id.ToString());

            using var resp = await http.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode)
            {
                delivery.MarkDelivered();
                _logger.LogInformation("Webhook delivered {DeliveryId} to {Url} ({Status})",
                    delivery.Id, subscription.Url, resp.StatusCode);
            }
            else
            {
                delivery.MarkFailed($"HTTP {(int)resp.StatusCode}", subscription.MaxRetries);
            }
        }
        catch (Exception ex)
        {
            delivery.MarkFailed(ex.Message, subscription.MaxRetries);
            _logger.LogWarning(ex, "Webhook delivery {DeliveryId} failed", delivery.Id);
        }

        return Result<WebhookDelivery>.Ok(delivery);
    }
}
