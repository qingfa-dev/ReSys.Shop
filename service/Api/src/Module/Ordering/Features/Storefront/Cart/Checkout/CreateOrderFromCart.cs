using Module.Inventory.Domain.Stock;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.Payments;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Ordering.Features.Storefront.Cart.Checkout;
/// <summary>Handles CreateOrderFromCart feature.</summary>
public static partial class CreateOrderFromCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        INotificationService notificationService,
        IOrderEventPublisher eventPublisher)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Resolve current user identifier.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Find the current user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .ThenInclude(x => x.Variant)
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

                var payment = await dbContext.Set<PaymentRecord>()
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

            // Validate: Stock availability for each item.
            foreach (var lineItem in cart.LineItems)
            {
                var stockItems = await dbContext.Set<StockItem>()
                    .Include(x => x.StockLocation)
                    .Where(x => x.VariantId == lineItem.VariantId)
                    .ToListAsync(cancellationToken);

                if (!AvailabilityValidator.IsAvailable(stockItems, lineItem.Quantity))
                    return StockItemResult.Errors.InsufficientStock;
            }

            // Update: Place the order.
            cart.Status = OrderStatus.Placed;
            cart.CheckoutState = CheckoutState.Complete;
            cart.CompletedAtUtc = DateTimeOffset.UtcNow;
            cart.Number = GenerateOrderNumber();

            foreach (var lineItem in cart.LineItems)
            {
                var stockItems = await dbContext.Set<StockItem>()
                    .Include(x => x.StockLocation)
                    .Where(si => si.VariantId == lineItem.VariantId)
                    .ToListAsync(cancellationToken);

                var remaining = lineItem.Quantity;
                foreach (var si in stockItems.OrderByDescending(s => s.CountOnHand))
                {
                    if (remaining <= 0) break;
                    var take = Math.Min(si.CountOnHand, remaining);
                    if (take > 0)
                    {
                        si.CountOnHand -= take;
                        remaining -= take;

                        var reservation = StockReservationExtensions.Reserve(
                            si.VariantId, take, si.StockLocationId, cart.Id, 30).Value;
                        dbContext.Set<StockReservation>().Add(reservation);

                        var movement = new StockMovement
                        {
                            Id = Guid.NewGuid(),
                            StockItemId = si.Id,
                            Quantity = -take,
                            PreviousCountOnHand = si.CountOnHand + take,
                            Action = "ship",
                            OriginatorType = "Order",
                            OriginatorId = cart.Id,
                            CreatedAtUtc = DateTimeOffset.UtcNow,
                            CreatedBy = currentUser.UserName
                        };
                        dbContext.Set<StockMovement>().Add(movement);
                    }
                }
            }

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Event: Publish order.placed to the event bus (no-op by default; wired to webhooks in Program.cs)
            await eventPublisher.PublishAsync("order.placed", new
            {
                OrderId = cart.Id,
                OrderNumber = cart.Number,
                UserId = cart.UserId,
                Email = cart.Email,
                Total = cart.Total,
                Currency = cart.Currency,
                PlacedAtUtc = cart.CompletedAtUtc
            }, cancellationToken);

            // Notify: Send order confirmation to customer.
            await SendOrderPlacedNotificationAsync(cart, cancellationToken);

            // Log: Success.
            OrderLoggers.Placed(logger, Number: cart.Number, Id: cart.Id, ActionBy: currentUser.UserName);

            // Map: Return the created order.
            return new Response
            {
                Id = cart.Id,
                Number = cart.Number,
                Status = cart.Status,
                PaymentState = cart.PaymentState
            };
        }

        private static string GenerateOrderNumber()
        {
            return $"R{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
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
                    order.Id, string.Join("; ", result.Failures.Select(f => f.Description)));
            }
        }
    }
}
