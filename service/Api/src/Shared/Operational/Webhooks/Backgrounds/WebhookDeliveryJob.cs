using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Webhooks.Domain;
using Shared.Operational.Webhooks.Services;

namespace Shared.Operational.Webhooks.Backgrounds;

public sealed class WebhookDeliveryJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWebhookDispatcher _dispatcher;
    private readonly ILogger<WebhookDeliveryJob> _logger;

    public WebhookDeliveryJob(
        IApplicationDbContext dbContext,
        IWebhookDispatcher dispatcher,
        ILogger<WebhookDeliveryJob> logger)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var due = await _dbContext.Set<WebhookDelivery>()
            .Where(d => (d.Status == WebhookDeliveryStatus.Pending
                         || (d.Status == WebhookDeliveryStatus.Failed && d.NextRetryAtUtc <= now)))
            .OrderBy(d => d.CreatedAtUtc)
            .Take(100)
            .ToListAsync(ct);

        _logger.LogDebug("Webhook delivery job picked {Count} deliveries", due.Count);

        foreach (var delivery in due)
        {
            var sub = await _dbContext.Set<WebhookSubscription>()
                .FirstOrDefaultAsync(s => s.Id == delivery.SubscriptionId, ct);
            if (sub is null || !sub.Active)
            {
                delivery.Status = WebhookDeliveryStatus.Dead;
                delivery.LastError = "Subscription not found or inactive";
                continue;
            }
            await _dispatcher.DeliverAsync(sub, delivery, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
