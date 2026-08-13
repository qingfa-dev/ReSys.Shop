using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Inventory.Services.StockReservations;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Features.Storefront.Cart.Checkout;
/// <summary>Converts the current user's draft cart into a placed order with payment verification, stock reservation consumption, notification, and event publishing.</summary>
public static partial class CreateOrderFromCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ISender sender,
        IStockReservationService stockReservationService)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates checkout prerequisites, verifies payment, consumes stock reservations, places the order, publishes an event, and sends a notification.</summary>
        /// <param name="command">The command containing checkout request with optional payment intent ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Resolve current user identifier.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the current user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Validate: Cart state must be Payment before completing checkout.
            if (cart.CheckoutState != CheckoutState.Payment)
                return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

            // Verify: Payment via ISender (replaces direct PaymentCapture query).
            var paymentIntentId = command.Request.PaymentIntentId!;
            var paymentResult = await sender.Send(
                new GetPaymentForCheckoutQuery { PaymentIntentId = paymentIntentId, OrderId = cart.Id }, cancellationToken);
            if (paymentResult.IsFailure)
                return paymentResult.Errors;

            var p = paymentResult.Value!;
            if (!p.IsCompleted || p.Amount <= 0)
                return OrderResult.Errors.PaymentNotCompleted;

            // Mark: Payment as paid via ISender (replaces domain MarkPaymentAsPaid).
            await sender.Send(new MarkPaymentPaidCommand { OrderId = cart.Id, PaymentIntentId = paymentIntentId }, cancellationToken);

            // Consume: Stock reservations via Inventory service (replaces inline CQRS handler).
            var consumeResult = await stockReservationService.ConsumeForOrderAsync(
                cart.Id, cancellationToken);
            if (consumeResult.IsFailure)
                return consumeResult.Errors;

            // Advance: Checkout state to Confirm (Place requires >= Confirm).
            var advanceToConfirmResult = cart.AdvanceCheckoutState(CheckoutState.Confirm);
            if (advanceToConfirmResult.IsFailure)
                return advanceToConfirmResult.Errors;

            // Generate: Unique order number.
            var numberResult = await OrderNumber.GenerateAsync(dbContext, cancellationToken);
            if (numberResult.IsFailure)
                return numberResult.Errors;

            // Place: Convert draft cart to placed order (sets state to Complete).
            var placeResult = cart.Place(numberResult.Value);
            if (placeResult.IsFailure)
                return placeResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Notify: Send order confirmation email to customer.
            await SendOrderPlacedNotificationAsync(cart, cancellationToken);

            // Log: Record placement in audit log.
            OrderLoggers.Placed(logger, Number: cart.Number, Id: cart.Id, ActionBy: currentUser.UserName);

            // Map: Return the created order as response.
            return Result<Response>.Created(cart.MapToDetail<Response>());
        }

        private async Task SendOrderPlacedNotificationAsync(Order order, CancellationToken ct)
        {
            // Skip: No email on order — nothing to notify.
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            // Notify: Send order confirmation with total and customer name.
            var message = NotificationMessage.Create(
                NotificationUseCase.OrderConfirmed,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.OrderTotal, order.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            // Suppress: Notification failure must not block order placement — best-effort only.
            var result = await notificationService.SendAsync(message, ct);
            if (result.IsFailure)
            {
                OrderLoggers.ConfirmationNotificationFailed(logger, order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}
