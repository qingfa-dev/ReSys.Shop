using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.Get.ById;

/// <summary>Handles retrieval of a stock location by identifier.</summary>
public static partial class GetStockLocationById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get stock location by id query.</summary>
        /// <param name="request">The query containing the stock location identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the stock location details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve stock location by identifier.
            var entity = await dbContext.Set<StockLocation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check: Verify the stock location exists.
            if (entity is null)
                return StockLocationResult.Errors.NotFound;

            // Map: Return the stock location as response.
            return entity.MapToDetail<Response>();
        }
    }
}
