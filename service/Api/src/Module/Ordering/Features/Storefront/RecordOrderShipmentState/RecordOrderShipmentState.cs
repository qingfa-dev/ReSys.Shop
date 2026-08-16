using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecordOrderShipmentState;

/// <summary>Writes the derived order fulfillment state and mirrors shipment timestamps (first-write).</summary>
public sealed class RecordOrderShipmentStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RecordOrderShipmentStateCommand>
{
    public async Task<Result> Handle(
        RecordOrderShipmentStateCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
        if (order is null)
            return OrderResult.Errors.NotFound(command.OrderId);

        if (command.ShippedAtUtc is not null)
            order.MarkShipped(command.ShippedAtUtc.Value);
        if (command.DeliveredAtUtc is not null)
            order.MarkDelivered(command.DeliveredAtUtc.Value);
        order.ShipmentState = command.FulfillmentState;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
