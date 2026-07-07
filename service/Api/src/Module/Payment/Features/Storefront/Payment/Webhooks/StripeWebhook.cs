using Module.Payment.Domain.Payments;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;
using Stripe;
using StripeEvent = Stripe.Event;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

    /// <summary>Handles StripeWebhook feature.</summary>
    public static partial class StripeWebhook
{
    public sealed record Command(string Payload, string StripeSignature) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStripeWebhookService webhookService)
        : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Validate: Verify Stripe webhook signature
            if (!webhookService.ValidateSignature(command.Payload, command.StripeSignature))
                return StripeWebhookResult.Errors.InvalidSignature;

            // Parse: Deserialize Stripe event
            var stripeEvent = webhookService.ParseEvent(command.Payload);
            if (stripeEvent is null)
                return StripeWebhookResult.Errors.InvalidPayload;

            // Process: Handle event by type
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    return await HandlePaymentIntentSucceeded(dbContext, stripeEvent, cancellationToken);

                case "payment_intent.payment_failed":
                    return await HandlePaymentIntentFailed(dbContext, stripeEvent, cancellationToken);

                case "charge.refunded":
                    return await HandleChargeRefunded(dbContext, stripeEvent, cancellationToken);

                case "charge.dispute.created":
                    return HandleChargeDisputeCreated(stripeEvent);

                default:
                    return Result.Ok();
            }
        }

        // Handle: payment_intent.succeeded -- transition payment to Completed
        private static async Task<Result> HandlePaymentIntentSucceeded(
            IApplicationDbContext dbContext, StripeEvent stripeEvent, CancellationToken cancellationToken)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null)
                return Result.Ok();

            var payment = await dbContext.Set<PaymentDomain>()
                .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, cancellationToken);

            if (payment is null)
                return Result.Ok();

            payment.Complete();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(PaymentResult.Success.Completed(payment.Number));
        }

        // Handle: payment_intent.payment_failed -- transition payment to Failed
        private static async Task<Result> HandlePaymentIntentFailed(
            IApplicationDbContext dbContext, StripeEvent stripeEvent, CancellationToken cancellationToken)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null)
                return Result.Ok();

            var payment = await dbContext.Set<PaymentDomain>()
                .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, cancellationToken);

            if (payment is null)
                return Result.Ok();

            payment.Fail();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(PaymentResult.Success.Failed(payment.Number));
        }

        // Handle: charge.refunded -- record refund
        private static async Task<Result> HandleChargeRefunded(
            IApplicationDbContext dbContext, StripeEvent stripeEvent, CancellationToken cancellationToken)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId))
                return Result.Ok();

            var payment = await dbContext.Set<PaymentDomain>()
                .FirstOrDefaultAsync(p => p.ResponseCode == charge.PaymentIntentId, cancellationToken);

            if (payment is null)
                return Result.Ok();

            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        // Handle: charge.dispute.created -- log dispute (no state change)
        private static Result HandleChargeDisputeCreated(StripeEvent stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge is null)
                return Result.Ok();

            // Log: Dispute created for informational purposes -- no payment state change
            return Result.Ok();
        }
    }
}
