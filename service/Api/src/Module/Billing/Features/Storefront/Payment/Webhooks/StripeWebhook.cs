using Hangfire;

using Module.Billing.Backgrounds;
using Module.Billing.Domain.WebhookEvents;

using IStripeWebhookService = Module.Billing.Services.Webhook.IStripeWebhookService;

namespace Module.Billing.Features.Storefront.Payment.Webhooks;

/// <summary>Processes an inbound Stripe webhook event.</summary>
public static partial class StripeWebhook
{
    public sealed record Command(string Payload, string StripeSignature) : ICommand;

    public sealed class CommandHandler(
        IStripeWebhookService webhookService,
        IApplicationDbContext dbContext,
        IBackgroundJobClient backgroundJobClient)
        : ICommandHandler<Command>
    {
        /// <summary>Processes an inbound Stripe webhook event.</summary>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Webhook: Validate HMAC-SHA256 signature — reject if invalid
            if (!webhookService.ValidateSignature(command.Payload, command.StripeSignature))
                return StripeWebhookResult.Errors.InvalidSignature;

            // Parse: Extract the Stripe event id + type before persisting
            var stripeEvent = webhookService.ParseEvent(command.Payload);
            if (stripeEvent is null)
                return StripeWebhookResult.Errors.InvalidPayload;

            // Persist: dedupe on the unique StripeEventId — a duplicate means the event
            // was already accepted; return Ok without re-enqueuing.
            var alreadyExists = await dbContext.Set<WebhookEvent>()
                .AnyAsync(e => e.StripeEventId == stripeEvent.Id, cancellationToken);
            if (alreadyExists)
                return Result.Ok("Webhook already accepted.");

            var webhookEvent = new WebhookEvent
            {
                StripeEventId = stripeEvent.Id,
                Type = stripeEvent.Type,
                Payload = command.Payload,
                State = WebhookEventState.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.Set<WebhookEvent>().Add(webhookEvent);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Unique-index race with a concurrent identical webhook — already accepted.
                return Result.Ok("Webhook already accepted.");
            }

            // Defer: Enqueue background job for async processing — avoids blocking webhook response
            // CancellationToken.None is a serialization placeholder — Hangfire injects the real token at execution time
            backgroundJobClient.Enqueue<ProcessStripeWebhookEventJob>(
                job => job.ExecuteAsync(webhookEvent.Id, CancellationToken.None));

            return Result.Ok("Webhook accepted and queued for processing.");
        }
    }
}