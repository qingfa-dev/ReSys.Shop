using Hangfire;

using Module.Payment.Backgrounds;

using IStripeWebhookService = Module.Payment.Services.Webhook.IStripeWebhookService;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

public static partial class StripeWebhook
{
    public sealed record Command(string Payload, string StripeSignature) : ICommand;

    public sealed class CommandHandler(
        IStripeWebhookService webhookService,
        IBackgroundJobClient backgroundJobClient)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!webhookService.ValidateSignature(command.Payload, command.StripeSignature))
                return StripeWebhookResult.Errors.InvalidSignature;

            var stripeEvent = webhookService.ParseEvent(command.Payload);
            if (stripeEvent is null)
                return StripeWebhookResult.Errors.InvalidPayload;

            backgroundJobClient.Enqueue<ProcessStripeWebhookEventJob>(
                job => job.ExecuteAsync(command.Payload, CancellationToken.None));

            return Result.Ok("Webhook accepted and queued for processing.");
        }
    }
}
