using Module.Ordering.Features.Storefront.RecordOrderShipmentState;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Services;

// Sync: recompute an order's derived fulfillment state and mirror it to Ordering (best-effort).
public sealed class ShipmentFulfillmentSyncService(
    IApplicationDbContext dbContext,
    ISender sender,
    ILogger<ShipmentFulfillmentSyncService> logger)
{
    public async Task SyncOrderFulfillmentAsync(Guid orderId, CancellationToken ct)
    {
        var shipments = await dbContext.Set<Shipment>()
            .Where(s => s.OrderId == orderId)
            .ToListAsync(ct);

        var state = ShipmentMethod.ComputeFulfillmentState(shipments.Select(s => s.Status).ToList());
        var shippedAt = shipments.Where(s => s.ShippedAtUtc is not null).Min(s => s.ShippedAtUtc);
        var deliveredAt = shipments.Where(s => s.DeliveredAtUtc is not null).Min(s => s.DeliveredAtUtc);

        var result = await sender.Send(new RecordOrderShipmentStateCommand
        {
            OrderId = orderId,
            FulfillmentState = state,
            ShippedAtUtc = shippedAt,
            DeliveredAtUtc = deliveredAt
        }, ct);
        if (result.IsFailure)
            logger.LogWarning("Failed to sync fulfillment for order {OrderId}: {Message}", orderId, result.Message);
    }
}
