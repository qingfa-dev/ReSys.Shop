using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Update;

/// <summary>Handles update of an existing stock item.</summary>
public static partial class UpdateStockItem
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the update stock item command.</summary>
        /// <param name="command">The command containing the update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the updated stock item details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Find the existing entity.
            var entity = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return StockItemResult.Errors.NotFound(command.Id);

            // Update: Map the request to update the entity.
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock item updated.
            StockItemLoggers.Adjusted(logger, CountOnHand: entity.CountOnHand, Id: entity.Id, ActionBy: currentUser.UserName);
            // Map: Return the updated stock item as response.
            return entity.MapToDetail<Response>();
        }
    }
}
