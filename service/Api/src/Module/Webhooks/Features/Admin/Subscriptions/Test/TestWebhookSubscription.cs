using System.Text.Json;

using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;
using Shared.Operational.Webhooks.Services;

namespace Module.Webhooks.Features.Admin.Subscriptions.Test;

public static partial class TestWebhookSubscription
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IWebhookDispatcher dispatcher)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var subscription = await dbContext.Set<WebhookSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            var samplePayload = JsonSerializer.Serialize(new
            {
                test = true,
                subscriptionId = subscription.Id,
                timestamp = DateTimeOffset.UtcNow,
            });

            var delivery = new WebhookDelivery
            {
                SubscriptionId = subscription.Id,
                Event = subscription.Event,
                PayloadJson = samplePayload,
                Status = WebhookDeliveryStatus.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            };

            dbContext.Set<WebhookDelivery>().Add(delivery);
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await dispatcher.DeliverAsync(subscription, delivery, cancellationToken);

            if (result.IsFailure)
            {
                return Result<Response>.Ok(new Response
                {
                    DeliveryId = delivery.Id,
                    Status = delivery.Status.ToString(),
                    AttemptCount = delivery.AttemptCount,
                    LastError = delivery.LastError,
                });
            }

            return Result<Response>.Ok(new Response
            {
                DeliveryId = delivery.Id,
                Status = delivery.Status.ToString(),
                AttemptCount = delivery.AttemptCount,
                LastError = delivery.LastError,
            });
        }
    }
}
