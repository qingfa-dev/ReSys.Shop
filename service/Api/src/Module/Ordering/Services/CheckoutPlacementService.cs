using Module.Ordering.Domain.Orders;
using Module.Inventory.Services.StockReservations;
using Module.Shipping.Features.Shared.Commands;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Services;

/// <summary>Places a draft order: consumes stock, advances to Confirm, generates a number, places, notifies.</summary>
public sealed class CheckoutPlacementService(
    IApplicationDbContext dbContext,
    IStockReservationService stockReservationService,
    INotificationService notificationService,
    ISender sender,
    ILogger<CheckoutPlacementService> logger)
{
    public async Task<Result<Order>> PlaceAsync(Order cart, string actor, CancellationToken ct)
    {
        logger.LogInformation("Placing order {OrderId} (actor={Actor})", cart.Id, actor);

        // Validate: checkout prerequisites BEFORE consuming stock, so a missing address,
        // shipping method, payment method or email can never strand already-consumed inventory.
        var advanceResult = cart.AdvanceCheckoutState(CheckoutState.Confirm);
        if (advanceResult.IsFailure) return advanceResult.Errors;

        var prerequisitesResult = cart.ValidateCheckoutPrerequisites();
        if (prerequisitesResult.IsFailure) return prerequisitesResult.Errors;

        var numberResult = await OrderNumber.GenerateAsync(dbContext, ct);
        if (numberResult.IsFailure) return numberResult.Errors;

        // Consume: fulfill stock for the order's line items. Resilient and idempotent —
        // missing/expired reservations are re-reserved on demand, and Fulfilled reservations
        // from a prior attempt are counted as already consumed.
        var lines = cart.LineItems
            .Select(li => new StockConsumeLine(li.VariantId, li.Quantity))
            .ToList();
        var consumeResult = await stockReservationService.ConsumeForOrderAsync(cart.Id, lines, ct);
        if (consumeResult.IsFailure) return consumeResult.Errors;

        var placeResult = cart.Place(numberResult.Value);
        if (placeResult.IsFailure) return placeResult.Errors;

        await dbContext.SaveChangesAsync(ct);

        await SendOrderPlacedNotificationAsync(cart, ct);

        // Auto-create: one Pending shipment for the placed order (best-effort — placement
        // is already committed; an admin can create a shipment later if this fails).
        if (cart.ShippingMethodId.HasValue)
        {
            try
            {
                // TODO(audit 2026-08-16): cross-module ISender — CreateShipmentCommand creates a foreign
                // aggregate; replace with a direct Shipping service call.
                var shipmentResult = await sender.Send(new CreateShipmentCommand
                {
                    OrderId = cart.Id,
                    ShippingMethodId = cart.ShippingMethodId.Value
                }, ct);
                if (shipmentResult.IsFailure)
                    logger.LogWarning("Failed to auto-create shipment for order {OrderId}: {Message}", cart.Id, shipmentResult.Message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to auto-create shipment for order {OrderId}", cart.Id);
            }
        }

        OrderLoggers.Placed(logger, Number: cart.Number, Id: cart.Id, ActionBy: actor);
        return Result<Order>.Ok(cart);
    }

    private async Task SendOrderPlacedNotificationAsync(Order order, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(order.Email)) return;

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
            OrderLoggers.ConfirmationNotificationFailed(logger, order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
    }
}
