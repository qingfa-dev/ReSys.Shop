using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.Shipments.Events;

namespace Module.Shipping.Features.Admin.Shipments.Ship;
/// <summary>Marks a shipment as shipped.</summary>
public static partial class MarkShipmentShipped
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles marking a shipment as shipped.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The shipment response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Get shipment by ID with order for customer info.
            var shipment = await dbContext.Set<Shipment>()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (shipment is null) return (Result<Response>)ShipmentResult.Errors.NotFound(command.Id);
            // Validate: Business rules.
            // Update: Mark as shipped.
            var result = shipment.Ship(command.Request.Tracking);
            if (result.IsFailure) return (Result<Response>)result.Failures;
            // Raise: Shipment shipped domain event with customer info for notification dispatch.
            shipment.AddDomainEvent(new ShipmentEvent.Lifecycle.Shipped(
                shipment.Id,
                shipment.OrderId,
                shipment.Tracking ?? string.Empty,
                null,
                shipment.ShippedAtUtc!.Value,
                shipment.Order.UserId!.Value,
                shipment.Order.Email ?? string.Empty));

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Log: Operation success.
            ShipmentLoggers.Shipped(logger, shipment.Number, shipment.Id, shipment.Tracking ?? string.Empty);
            // Map: Return response.
            return new Response { Id = shipment.Id, Number = shipment.Number, State = shipment.State, Tracking = shipment.Tracking, ShippedAtUtc = shipment.ShippedAtUtc };
        }
    }
}
