using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Services;

namespace Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;

public sealed class CompleteCheckoutForPaymentCommandHandler(
    IApplicationDbContext dbContext,
    ISender sender,
    CheckoutPlacementService placementService,
    ILogger<CompleteCheckoutForPaymentCommandHandler> logger)
    : ICommandHandler<CompleteCheckoutForPaymentCommand, CompleteCheckoutForPaymentResponse>
{
    public async Task<Result<CompleteCheckoutForPaymentResponse>> Handle(
        CompleteCheckoutForPaymentCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .Where(x => x.Id == command.CartId && x.Status == OrderStatus.Draft)
            .FirstOrDefaultAsync(cancellationToken);

        logger.LogDebug("CompleteCheckoutForPayment: CartId={CartId}, PaymentId={PaymentId}, CartFound={Found}", command.CartId, command.PaymentId, cart is not null);

        // Idempotency: a no-longer-draft order was already placed by an earlier retry.
        if (cart is null)
            return new CompleteCheckoutForPaymentResponse { OrderId = command.CartId, Placed = false };

        // Self-defend: never place an order whose payment is not yet Completed.
        // TODO(audit 2026-08-16): cross-module ISender — GetPaymentForCheckoutQuery reads a
        // PaymentCapture reachable via Order.PaymentCaptures; use .Include + local filter.
        var paymentResult = await sender.Send(
            new GetPaymentForCheckoutQuery
            {
                PaymentIntentId = command.PaymentId.ToString(),
                OrderId = command.CartId
            }, cancellationToken);
        if (paymentResult.IsFailure || paymentResult.Value is not { IsCompleted: true })
            return OrderResult.Errors.PaymentNotCompleted;

        // Timestamp: mirror the payment completion onto the order's payment timeline.
        cart.MarkPaymentCompleted(paymentResult.Value.CompletedAtUtc ?? DateTimeOffset.UtcNow);

        // Record: persist the payment method onto the order before finalization —
        // the checkout prerequisite requires it, and the Billing capture is its source.
        if (paymentResult.Value.PaymentMethodId.HasValue)
            cart.PaymentMethodId = paymentResult.Value.PaymentMethodId;

        if (cart.CheckoutState != CheckoutState.PickPaymentMethod)
            return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

        var placeResult = await placementService.PlaceAsync(cart, "System", cancellationToken);
        if (placeResult.IsFailure)
        {
            logger.LogWarning("Webhook auto-placement failed: CartId={CartId}: {Message}", command.CartId, placeResult.Message);
            return placeResult.Errors;
        }

        logger.LogInformation("Order auto-placed from webhook: CartId={CartId}", command.CartId);
        return new CompleteCheckoutForPaymentResponse { OrderId = cart.Id, Placed = true };
    }
}
