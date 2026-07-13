using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockMovements.Get.ById;

/// <summary>Gets a single stock movement record by its unique identifier.</summary>
public static partial class GetStockMovementById
{
    public sealed record Query(Guid Id) : ICommand<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Query, Response>
    {
        /// <summary>Fetches the stock movement with no-tracking for read-only access.</summary>
        /// <param name="request">The query containing the stock movement identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the stock movement details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Load: Retrieve stock movement by identifier.
            var entity = await dbContext.Set<StockMovement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity is null)
                return StockMovementResult.Errors.StockItemNotFound;

            // Map: Return the stock movement as response.
            return entity.MapToDetail<Response>();
        }
    }
}