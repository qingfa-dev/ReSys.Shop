using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Admin.Orders.Shipments.Create;
/// <summary>Handles CreateOrderShipment feature.</summary>
public static partial class CreateOrderShipment
{
    public sealed record Command(Guid OrderId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Create: Build the shipment.
            var createResult = ShipmentExtensions.Create(command.OrderId, command.Request.StockLocationId);
            if (createResult.IsFailure)
                return createResult.Failures;

            var shipment = createResult.Value;

            // Update: Set optional shipping method.
            shipment.ShippingMethodId = command.Request.ShippingMethodId;

            // Persist: Save the new entity.
            dbContext.Set<Shipment>().Add(shipment);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return new Response { Id = shipment.Id, Number = shipment.Number, OrderId = shipment.OrderId, StockLocationId = shipment.StockLocationId, CreatedAtUtc = shipment.CreatedAtUtc };
        }
    }
}
