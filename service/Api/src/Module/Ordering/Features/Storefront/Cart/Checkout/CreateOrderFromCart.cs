using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Extensions;
using Module.Ordering.Features.Admin.Shared.Mappings;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Module.Ordering.Services;
using Module.Ordering.Features.Storefront.Shared.Services;

namespace Module.Ordering.Features.Storefront.Cart.Checkout;

public static partial class CreateOrderFromCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ISender sender,
        CheckoutPlacementService placementService)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            var cart = await dbContext.Set<Order>()
                .IncludeOrderDetail()
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);
            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            if (cart.CheckoutState != CheckoutState.PickPaymentMethod)
                return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Placed);

            // TODO(audit 2026-08-16): cross-module ISender — replace with direct navigation.
            // GetPaymentForCheckoutQuery reads a PaymentCapture reachable via Order.PaymentCaptures
            // (.Include(x => x.PaymentCaptures) + local filter). See AGENTS.md rule #2 candidates.
            var paymentResult = await sender.Send(
                new GetPaymentForCheckoutQuery { PaymentIntentId = command.Request.PaymentIntentId!, OrderId = cart.Id }, cancellationToken);
            if (paymentResult.IsFailure)
                return paymentResult.Errors;

            var p = paymentResult.Value!;
            var isPaid = p.IsCompleted || (p.IsPending && p.IsOffline);
            if (!isPaid || p.Amount <= 0)
                return OrderResult.Errors.PaymentNotCompleted;

            // TODO(audit 2026-08-16): cross-module ISender — MarkPaymentPaidCommand wraps one guard +
            // payment.Complete(); call payment.Complete() on the nav-loaded capture directly.
            // COD stays Pending: only gateway-completed payments are marked paid here.
            if (!p.IsOffline)
                await sender.Send(new MarkPaymentPaidCommand
                {
                    OrderId = cart.Id,
                    PaymentIntentId = command.Request.PaymentIntentId!
                }, cancellationToken);

            // Timestamp: mirror the (already completed) payment onto the order timeline.
            if (p.IsCompleted)
                cart.MarkPaymentCompleted(p.CompletedAtUtc ?? DateTimeOffset.UtcNow);

            // Record: persist the payment method onto the order before finalization —
            // the checkout prerequisite requires it, and the Billing capture is its source.
            if (p.PaymentMethodId.HasValue)
                cart.PaymentMethodId = p.PaymentMethodId;

            var placeResult = await placementService.PlaceAsync(cart, currentUser.UserName!, cancellationToken);
            if (placeResult.IsFailure)
                return placeResult.Errors;

            // Enrich: Resolve product references (id, name, primary image) for the placed order's line items.
            var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
            var itemLookup = await ProductLookupFactory.BuildAsync(dbContext, variantIds, cancellationToken);

            return Result<Response>.Created(
                cart.MapToDetailWithLookup<Response>(itemLookup));
        }
    }
}
