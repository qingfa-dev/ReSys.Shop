using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Get.ById;

/// <summary>Handles retrieval of a stock item by identifier.</summary>
public static partial class GetStockItemById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get stock item by id query.</summary>
        /// <param name="request">The query containing the stock item identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the stock item details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve stock item by identifier.
            var entity = await dbContext.Set<StockItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check: Verify the stock item exists.
            if (entity is null)
                return StockItemResult.Errors.NotFound(request.Id);

            // Map: Return the stock item as response.
            return entity.MapToDetail<Response>();
        }
    }
}
