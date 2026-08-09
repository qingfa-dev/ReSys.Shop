using Hangfire;

using Module.Billing.Backgrounds;

using IStripeWebhookService = Module.Billing.Services.Webhook.IStripeWebhookService;

namespace Module.Billing.Features.Storefront.Payment.Webhooks;

/// <summary>Processes an inbound Stripe webhook event.</summary>
public static partial class StripeWebhook
{
    public sealed record Command(string Payload, string StripeSignature) : ICommand;

    public sealed class CommandHandler(
        IStripeWebhookService webhookService,
        IBackgroundJobClient backgroundJobClient)
        : ICommandHandler<Command>
    {
        /// <summary>Processes an inbound Stripe webhook event.</summary>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Webhook: Validate HMAC-SHA256 signature — reject if invalid
            if (!webhookService.ValidateSignature(command.Payload, command.StripeSignature))
                return StripeWebhookResult.Errors.InvalidSignature;

            // Defer: Enqueue background job for async processing — avoids blocking webhook response
            // CancellationToken.None is a serialization placeholder — Hangfire injects the real token at execution time
            backgroundJobClient.Enqueue<ProcessStripeWebhookEventJob>(
                job => job.ExecuteAsync(command.Payload, CancellationToken.None));

            return Result.Ok("Webhook accepted and queued for processing.");
        }
    }
}