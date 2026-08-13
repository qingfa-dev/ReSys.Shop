using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Module.Ordering.Services;

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
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);
            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            if (cart.CheckoutState != CheckoutState.Payment)
                return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

            var paymentResult = await sender.Send(
                new GetPaymentForCheckoutQuery { PaymentIntentId = command.Request.PaymentIntentId!, OrderId = cart.Id }, cancellationToken);
            if (paymentResult.IsFailure)
                return paymentResult.Errors;

            var p = paymentResult.Value!;
            var isPaid = p.IsCompleted || (p.State == "Pending" && p.IsOffline);
            if (!isPaid || p.Amount <= 0)
                return OrderResult.Errors.PaymentNotCompleted;

            // COD stays Pending: only gateway-completed payments are marked paid here.
            if (!p.IsOffline)
                await sender.Send(new MarkPaymentPaidCommand
                {
                    OrderId = cart.Id,
                    PaymentIntentId = command.Request.PaymentIntentId!
                }, cancellationToken);

            var placeResult = await placementService.PlaceAsync(cart, currentUser.UserName!, cancellationToken);
            if (placeResult.IsFailure)
                return placeResult.Errors;

            return Result<Response>.Created(cart.MapToDetail<Response>());
        }
    }
}
