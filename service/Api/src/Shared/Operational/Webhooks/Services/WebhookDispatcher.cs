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
            var delivery = new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                SubscriptionId = sub.Id,
                Event = eventName,
                PayloadJson = payloadJson,
                Status = WebhookDeliveryStatus.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            _dbContext.Set<WebhookDelivery>().Add(delivery);
        }
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<WebhookDelivery>> DeliverAsync(
        WebhookSubscription subscription, WebhookDelivery delivery, CancellationToken ct = default)
    {
        delivery.AttemptCount += 1;
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
                delivery.Status = WebhookDeliveryStatus.Delivered;
                delivery.DeliveredAtUtc = DateTimeOffset.UtcNow;
                _logger.LogInformation("Webhook delivered {DeliveryId} to {Url} ({Status})",
                    delivery.Id, subscription.Url, resp.StatusCode);
            }
            else
            {
                HandleFailure(delivery, subscription, $"HTTP {(int)resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            HandleFailure(delivery, subscription, ex.Message);
            _logger.LogWarning(ex, "Webhook delivery {DeliveryId} failed", delivery.Id);
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result<WebhookDelivery>.Ok(delivery);
    }

    private static void HandleFailure(WebhookDelivery delivery, WebhookSubscription sub, string error)
    {
        delivery.LastError = error;
        if (delivery.AttemptCount >= sub.MaxRetries)
        {
            delivery.Status = WebhookDeliveryStatus.Dead;
        }
        else
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            var delaySeconds = (int)Math.Pow(5, delivery.AttemptCount) * 60;
            delivery.NextRetryAtUtc = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
        }
    }
}
