using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.UpdateTracking;
/// <summary>Updates the tracking number for a shipment.</summary>
public static partial class UpdateShipmentTracking
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles updating shipment tracking.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated tracking response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            _ = logger;
            // Query: Get shipment by ID.
            var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (shipment is null) return (Result<Response>)ShipmentResult.Errors.NotFound(command.Id);
            // Update: Set tracking number.
            shipment.Tracking = command.Request.Tracking;
            shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Map: Return response.
            return new Response { Id = shipment.Id, Tracking = shipment.Tracking, ModifiedAtUtc = shipment.ModifiedAtUtc };
        }
    }
}
