using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.Delete;
/// <summary>Deletes a shipment by ID.</summary>
public static partial class DeleteShipment
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        /// <summary>Handles deleting a shipment.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            _ = logger;
            // Query: Get shipment by ID.
            var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (shipment is null) return (Result)ShipmentResult.Errors.NotFound(command.Id);
            // Remove: Delete the shipment.
            dbContext.Set<Shipment>().Remove(shipment);
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
