using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Create;

/// <summary>Creates a stock item for a product variant at a specific location after ensuring no duplicate exists.</summary>
public static partial class CreateStockItem
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Creates a new stock item for a product variant at the specified location.</summary>
        /// <param name="command">The command containing stock item data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the created stock item details.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
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