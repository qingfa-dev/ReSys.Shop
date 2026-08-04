using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

/// <summary>Updates the shipment state of an order (e.g., pending, delivered, partial, ready, backorder, canceled).</summary>
public static partial class UpdateOrderShipmentState
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Updates the shipment state of an order and returns the updated order detail.</summary>
        /// <param name="command">The command containing the order ID and new shipment state.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            var validStates = new[]
            {
                OrderConstant.ShipmentState.Pending,
                OrderConstant.ShipmentState.Delivered,
                OrderConstant.ShipmentState.Partial,
                OrderConstant.ShipmentState.Ready,
                OrderConstant.ShipmentState.Backorder,
                OrderConstant.ShipmentState.Canceled,
            };

            if (!validStates.Contains(command.Request.ShipmentState))
                return OrderResult.Errors.InvalidShipmentState;

            order.ShipmentState = command.Request.ShipmentState;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Ok(order.MapToDetail<Response>(), OrderResult.Success.ShipmentStateUpdated(order.Id));
        }
    }
}
