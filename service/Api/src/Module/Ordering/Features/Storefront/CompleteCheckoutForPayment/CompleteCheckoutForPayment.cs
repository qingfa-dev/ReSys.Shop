using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Services;

namespace Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;

public sealed class CompleteCheckoutForPaymentCommandHandler(
    IApplicationDbContext dbContext,
    ISender sender,
    CheckoutPlacementService placementService)
    : ICommandHandler<CompleteCheckoutForPaymentCommand, CompleteCheckoutForPaymentResponse>
{
    public async Task<Result<CompleteCheckoutForPaymentResponse>> Handle(
        CompleteCheckoutForPaymentCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .Where(x => x.Id == command.CartId && x.Status == OrderStatus.Draft)
            .FirstOrDefaultAsync(cancellationToken);

        // Idempotency: a no-longer-draft order was already placed by an earlier retry.
        if (cart is null)
            return new CompleteCheckoutForPaymentResponse { OrderId = command.CartId };

        // Self-defend: never place an order whose payment is not yet Completed.
        var paymentResult = await sender.Send(
            new GetPaymentForCheckoutQuery
            {
                PaymentIntentId = command.PaymentId.ToString(),
                OrderId = command.CartId
            }, cancellationToken);
        if (paymentResult.IsFailure || paymentResult.Value is not { IsCompleted: true })
            return OrderResult.Errors.PaymentNotCompleted;

        if (cart.CheckoutState != CheckoutState.Payment)
            return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

        var placeResult = await placementService.PlaceAsync(cart, "System", cancellationToken);
        if (placeResult.IsFailure)
            return placeResult.Errors;

        return new CompleteCheckoutForPaymentResponse { OrderId = cart.Id };
    }
}
