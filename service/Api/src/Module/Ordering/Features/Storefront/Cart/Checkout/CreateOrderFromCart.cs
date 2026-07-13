using System.Data;

using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Ordering.Domain.Adjustments;
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

            // Check: Find the current user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Validate: Checkout prerequisites (addresses, shipping method, email).
            var prereqResult = cart.ValidateCheckoutPrerequisites();
            if (prereqResult.IsFailure)
                return prereqResult.Errors;

            // Validate: Payment.
            var paymentIntentId = command.Request.PaymentIntentId;
            var payment = !string.IsNullOrWhiteSpace(paymentIntentId)
                ? await dbContext.Set<PaymentCapture>()
                    .FirstOrDefaultAsync(p => p.ResponseCode == paymentIntentId
                                          && p.OrderId == cart.Id
                                          && p.State == PaymentRecordState.Completed, cancellationToken)
                : null;

            var paymentResult = cart.ValidatePayment(
                payment?.Amount ?? 0m,
                payment?.State == PaymentRecordState.Completed);
            if (paymentResult.IsFailure)
                return paymentResult.Errors;

            var paymentMarkResult = cart.MarkPaymentAsPaid();
            if (paymentMarkResult.IsFailure)
                return paymentMarkResult.Errors;

            // Validate: Reject orders containing discontinued variants.
            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var discontinuedVariantIds = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id) && v.DiscontinuedOn != null)
                .Select(v => v.Id)
                .ToHashSetAsync(cancellationToken);

            if (!cart.EnsureLineItemVariantsAreNotDiscontinued(discontinuedVariantIds))
                return OrderResult.Errors.VariantDiscontinued;

            // Explain: RepeatableRead ensures stock rows read for deduction are stable
            // during the transaction — prevents stock double-deduction under concurrent checkouts.
            await using var transaction = await dbContext.BeginTransactionAsync(
                IsolationLevel.RepeatableRead, cancellationToken);
            try
            {
                // Generate: Unique order number inside transaction so rollback doesn't leak numbers
                var numberResult = await OrderNumber.GenerateAsync(dbContext, cancellationToken);
                if (numberResult.IsFailure)
                    return numberResult.Errors;
                var placeResult = cart.Place(numberResult.Value);
                if (placeResult.IsFailure)
                    return placeResult.Errors;

                foreach (var lineItem in cart.LineItems)
                {
                    // Deduct: Consume stock from locations with highest on-hand first.
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

                        // Update: Deduct stock from this location.
                        si.CountOnHand -= take;
                        si.ModifiedAtUtc = DateTimeOffset.UtcNow;

                        remaining -= take;

                        // Create: Reserve stock for this order (30-day expiry).
                        const int StockReservationExpiryDays = 30;
                        var reservation = StockReservationMethod.Reserve(
                            si.VariantId, take, si.StockLocationId, cart.Id, StockReservationExpiryDays).Value;
                        dbContext.Set<StockReservation>().Add(reservation);

                        // Log: Record stock movement for audit trail.
                        var movementResult = StockMovementMethod.Create(
                            stockItemId: si.Id,
                            quantity: -take,
                            previousCountOnHand: si.CountOnHand,
                            originatorType: AdjustmentConstant.AdjustableTypes.Order,
                            originatorId: cart.Id,
                            action: OrderConstant.StockAction.Ship,
                            createdBy: currentUser.UserName ?? "System");

                        if (movementResult.IsSuccess)
                            dbContext.Set<StockMovement>().Add(movementResult.Value);
                    }

                    if (remaining > 0)
                        return StockItemResult.Errors.InsufficientStock;
                }

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return StockItemResult.Errors.ConcurrencyConflict(
                        cart.LineItems.First().VariantId);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

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
                logger.LogWarning("Failed to send order confirmation notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
            }
        }
    }
}