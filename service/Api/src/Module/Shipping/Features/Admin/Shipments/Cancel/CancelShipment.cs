using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.Cancel;
/// <summary>Cancels a shipment by ID.</summary>
public static partial class CancelShipment
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        /// <summary>Handles cancelling a shipment.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Query: Get shipment by ID.
            var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (shipment is null) return (Result)ShipmentResult.Errors.NotFound(command.Id);
            // Update: Cancel the shipment.
            var result = shipment.Cancel();
            if (result.IsFailure) return result.Failures;
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Log: Operation success.
            ShipmentLoggers.Canceled(logger, shipment.Number, shipment.Id);
            return Result.Ok();
        }
    }
}
