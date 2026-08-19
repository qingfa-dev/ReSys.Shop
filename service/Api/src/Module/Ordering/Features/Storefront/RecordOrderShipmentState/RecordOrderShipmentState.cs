using Module.Ordering.Domain.Orders;
using Shared.Application.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecordOrderShipmentState;

/// <summary>Writes the derived order fulfillment state and mirrors shipment timestamps (first-write).</summary>
public sealed class RecordOrderShipmentStateCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<RecordOrderShipmentStateCommandHandler> logger)
    : ICommandHandler<RecordOrderShipmentStateCommand>
{
    public async Task<Result> Handle(
        RecordOrderShipmentStateCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
        if (order is null)
            return OrderResult.Errors.NotFound(command.OrderId);

        // Guard: Terminal/abnormal orders cannot accept derived shipment state — no-op, no write.
        if (order.Status is OrderStatus.Canceled or OrderStatus.Completed or OrderStatus.Expired)
            return Result.Ok();

        if (command.ShippedAtUtc is not null)
            order.MarkShipped(command.ShippedAtUtc.Value);
        if (command.DeliveredAtUtc is not null)
            order.MarkDelivered(command.DeliveredAtUtc.Value);
        order.ShipmentState = command.FulfillmentState;

        // Derive: Auto-complete when fully delivered and still in Placed state
        if (command.FulfillmentState == ShipmentState.Delivered && order.Status == OrderStatus.Placed)
        {
            var completeResult = order.Complete("System");
            if (completeResult.IsFailure)
            {
                OrderLoggers.CompletionFailed(logger, order.Id, string.Join("; ", completeResult.Errors.Select(f => f.Message)));
                return completeResult.Errors;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
