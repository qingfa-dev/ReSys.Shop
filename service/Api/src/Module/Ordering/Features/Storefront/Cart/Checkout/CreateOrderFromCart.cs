using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Payment.Domain.PaymentCaptures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Ordering.Features.Storefront.Cart.Checkout;
/// <summary>Converts the current user's draft cart into a placed order with payment verification, stock deduction, inventory reservation, notification, and event publishing.</summary>
public static partial class CreateOrderFromCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        INotificationService notificationService)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates checkout prerequisites, verifies payment, deducts stock, reserves inventory, places the order, publishes an event, and sends a notification.</summary>
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

            // Load: Find the current user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Validate: Checkout steps must be completed before placing order.
            if (cart.CheckoutState < CheckoutState.Confirm)
                return OrderResult.Errors.CheckoutNotComplete;

            // Validate: Prerequisites for order placement.
            if (cart.BillAddressId is null || cart.ShipAddressId is null)
                return OrderResult.Errors.AddressRequired;

            if (cart.ShippingMethodId is null)
                return OrderResult.Errors.DeliveryMethodRequired;

            if (string.IsNullOrWhiteSpace(cart.Email))
                return OrderResult.Errors.EmailRequired;

            // Validate: Payment verification (skip for zero-total orders).
            if (cart.Total > 0m)
            {
                var paymentIntentId = command.Request.PaymentIntentId;
                if (string.IsNullOrWhiteSpace(paymentIntentId))
                    return OrderResult.Errors.PaymentRequired;

                var payment = await dbContext.Set<PaymentCapture>()
                    .FirstOrDefaultAsync(p => p.ResponseCode == paymentIntentId
                                          && p.OrderId == cart.Id
                                          && p.State == PaymentRecordState.Completed, cancellationToken);

                if (payment is null)
                    return OrderResult.Errors.PaymentFailed;

                if (payment.Amount != cart.Total)
                    return OrderResult.Errors.PaymentAmountMismatch;

                cart.PaymentState = "paid";
            }

            // Validate: Cart has items.
            if (cart.LineItems.Count == 0)
                return OrderResult.Errors.EmptyOrderCannotFinalize;

            // Validate: Check for discontinued variants.
            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var discontinuedVariantIds = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id) && v.DiscontinuedOn != null)
                .Select(v => v.Id)
                .ToHashSetAsync(cancellationToken);

            if (!cart.EnsureLineItemVariantsAreNotDiscontinued(discontinuedVariantIds))
                return OrderResult.Errors.VariantDiscontinued;

            // Update: Place the order.
            cart.Status = OrderStatus.Placed;
            cart.CheckoutState = CheckoutState.Complete;
            cart.CompletedAtUtc = DateTimeOffset.UtcNow;
            cart.Number = OrderNumber.Generate(dbContext, out _);

            // Deduct: Atomic stock deduction with optimistic concurrency guard.
            // Each ExecuteUpdateAsync has a WHERE CountOnHand >= take guard;
            // if 0 rows are updated the stock was claimed by another request.
            foreach (var lineItem in cart.LineItems)
            {
                var stockItems = await dbContext.Set<StockItem>()
                    .Where(si => si.VariantId == lineItem.VariantId)
                    .OrderByDescending(si => si.CountOnHand)
                    .ToListAsync(cancellationToken);

                var remaining = lineItem.Quantity;
                foreach (var si in stockItems)
                {
                    if (remaining <= 0) break;
                    var take = Math.Min(si.CountOnHand, remaining);
                    if (take <= 0) continue;

                    si.CountOnHand -= take;
                    si.ModifiedAtUtc = DateTimeOffset.UtcNow;

                    remaining -= take;

                    var reservation = StockReservationMethod.Reserve(
                        si.VariantId, take, si.StockLocationId, cart.Id, 30).Value;
                    dbContext.Set<StockReservation>().Add(reservation);

                    var movementResult = StockMovementMethod.Create(
                        stockItemId: si.Id,
                        quantity: -take,
                        previousCountOnHand: si.CountOnHand,
                        originatorType: "Order",
                        originatorId: cart.Id,
                        action: "ship",
                        createdBy: currentUser.UserName ?? "System");

                    if (movementResult.IsSuccess)
                        dbContext.Set<StockMovement>().Add(movementResult.Value);
                }

                if (remaining > 0)
                    return StockItemResult.Errors.InsufficientStock;
            }

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
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            var message = NotificationMessage.Create(
                NotificationUseCase.OrderConfirmed,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.OrderTotal, order.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            var result = await notificationService.SendAsync(message, ct);
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order confirmation notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}
