using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Mappings;

namespace Module.Shipping.Features.Admin.Shipments.Create;
/// <summary>Creates a new shipment for an order.</summary>
public static partial class CreateShipment
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles creating a shipment.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created shipment response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Build the shipment via mapping.
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Failures;

            var shipment = createResult.Value;
            shipment.ShippingMethodId = request.ShippingMethodId;

            // Persist: Save changes.
            dbContext.Set<Shipment>().Add(shipment);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Operation success.
            ShipmentLoggers.Created(logger, shipment.Number, shipment.Id, shipment.OrderId);

            // Map: Return response.
            return Result<Response>.Ok(shipment.MapToDetail<Response>(), "Shipment created successfully.");
        }
    }
}
