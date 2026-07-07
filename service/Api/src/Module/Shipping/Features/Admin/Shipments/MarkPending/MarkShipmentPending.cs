using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.MarkPending;
/// <summary>Marks a shipment as pending.</summary>
public static partial class MarkShipmentPending
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        /// <summary>Handles marking a shipment as pending.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            _ = logger;
            // Check: Get shipment by ID.
            var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (shipment is null) return (Result)ShipmentResult.Errors.NotFound(command.Id);
            // Validate: Business rules.
            // Update: Set shipment to pending.
            var result = shipment.Pend();
            if (result.IsFailure) return result.Failures;
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Log: Operation success.
            return Result.Ok();
        }
    }
}
