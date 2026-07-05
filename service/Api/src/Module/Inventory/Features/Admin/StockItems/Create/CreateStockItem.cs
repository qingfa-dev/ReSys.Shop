using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Create;

/// <summary>Handles creation of a new stock item.</summary>
public static partial class CreateStockItem
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the create stock item command.</summary>
        /// <param name="command">The command containing the stock item request data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result indicating success with the created response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Verify if a stock item already exists for the variant and location combination.
            var exists = await dbContext.Set<StockItem>()
                .AnyAsync(x => x.VariantId == request.VariantId
                    && x.StockLocationId == request.StockLocationId, cancellationToken);

            if (exists)
                return StockItemResult.Errors.AlreadyExists(request.VariantId, request.StockLocationId);

            // Create: Map the request to a new StockItem entity.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            // Persist: Save the new stock item to the database.
            dbContext.Set<StockItem>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Stock item created.
            StockItemLoggers.Created(logger, VariantId: entity.VariantId, StockLocationId: entity.StockLocationId, Id: entity.Id, ActionBy: currentUser.UserName);
            // Map: Return the created stock item as response.
            return Result<Response>.Created(
                entity.MapToDetail<Response>(),
                StockItemResult.Success.Created(entity.Id));
        }
    }
}
