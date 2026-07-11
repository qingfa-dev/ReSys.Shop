using System.Text.Json;

using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;
using Shared.Operational.Webhooks.Services;

namespace Module.Webhooks.Features.Admin.Subscriptions.Test;

/// <summary>Sends a test webhook delivery to verify the subscription configuration.</summary>
public static partial class TestWebhookSubscription
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IWebhookDispatcher dispatcher)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Loads the subscription, creates a delivery record, sends a test payload, and returns the result.</summary>
        /// <param name="command">The command identifying the subscription to test.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the test delivery status and any error details.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=subscription!=null, post=delivery sent and recorded, throws=DbUpdateException
            // Load: Find the subscription by ID (no-tracking for read-only)
            var subscription = await dbContext.Set<WebhookSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            // Generate: Sample test payload for verification
            var samplePayload = JsonSerializer.Serialize(new
            {
                test = true,
                subscriptionId = subscription.Id,
                timestamp = DateTimeOffset.UtcNow,
            });

            // Create: Build delivery record
            var deliveryResult = WebhookDeliveryMethod.Create(
                subscriptionId: subscription.Id,
                @event: subscription.Event,
                payloadJson: samplePayload);
            if (deliveryResult.IsFailure) return Result<Response>.Failure(deliveryResult.Errors[0]);
            var delivery = deliveryResult.Value;

            dbContext.Set<WebhookDelivery>().Add(delivery);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Send: Dispatch the test payload to the subscription endpoint
            var result = await dispatcher.DeliverAsync(subscription, delivery, cancellationToken);

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
