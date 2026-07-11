using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;
using Stripe;
using StripeEvent = Stripe.Event;
using Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Webhooks;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

/// <summary>Processes inbound Stripe webhooks by validating signatures and dispatching to event handlers.</summary>
public static partial class StripeWebhook
{
    public sealed record Command(string Payload, string StripeSignature) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStripeWebhookService webhookService)
        : ICommandHandler<Command>
    {
        /// <summary>Validates the Stripe webhook signature, parses the event, and routes to the appropriate handler.</summary>
        /// <param name="command">The command containing the raw payload and Stripe signature header.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or an error if signature validation or event processing fails.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails in event handlers.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && command.Payload!=null, post=event routed or silently skipped,
            //           throws=DbUpdateException
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
                case GatewayConstants.WebhookEvents.Stripe.PaymentIntentSucceeded:
                    return await HandlePaymentIntentSucceeded(dbContext, stripeEvent, cancellationToken);

                case GatewayConstants.WebhookEvents.Stripe.PaymentIntentPaymentFailed:
                    return await HandlePaymentIntentFailed(dbContext, stripeEvent, cancellationToken);

                case GatewayConstants.WebhookEvents.Stripe.ChargeRefunded:
                    return await HandleChargeRefunded(dbContext, stripeEvent, cancellationToken);

                case GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated:
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

            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, cancellationToken);

            if (payment is null)
                return Result.Ok();

            var completeResult = payment.Complete();
            if (completeResult.IsFailure)
                return completeResult.Errors;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(PaymentCaptureResult.Success.Completed(payment.Number));
        }

        // Handle: payment_intent.payment_failed -- transition payment to Failed
        private static async Task<Result> HandlePaymentIntentFailed(
            IApplicationDbContext dbContext, StripeEvent stripeEvent, CancellationToken cancellationToken)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent is null)
                return Result.Ok();

            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, cancellationToken);

            if (payment is null)
                return Result.Ok();

            var failResult = payment.Fail();
            if (failResult.IsFailure)
                return failResult.Errors;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(PaymentCaptureResult.Success.Failed(payment.Number));
        }

        // Handle: charge.refunded -- record refund
        private static async Task<Result> HandleChargeRefunded(
            IApplicationDbContext dbContext, StripeEvent stripeEvent, CancellationToken cancellationToken)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId))
                return Result.Ok();

            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.ResponseCode == charge.PaymentIntentId, cancellationToken);

            if (payment is null)
                return Result.Ok();

            if (charge.AmountRefunded > 0)
            {
                var newRefunded = charge.AmountRefunded / 100m;
                var delta = newRefunded - payment.RefundedAmount;
                if (delta > 0)
                    payment.Refund(delta);
            }

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

            // TODO: Add dispute handling logic when business requirements are defined
            return Result.Ok();
        }
    }
}
