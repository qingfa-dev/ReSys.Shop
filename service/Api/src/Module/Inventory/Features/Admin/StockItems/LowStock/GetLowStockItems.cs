using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Features.Admin.StockItems.LowStock;

/// <summary>Handles retrieval of stock items below their location's low-stock threshold.</summary>
public static partial class GetLowStockItems
{
    public sealed record Query(Guid? LocationId, int? Threshold) : IQuery<List<Response>>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Executes the get low stock items query.</summary>
        /// <param name="request">The query containing optional location and threshold filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of stock items below their threshold.</returns>
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve stock items with location filter
            var query = dbContext.Set<StockItem>()
                .Include(si => si.StockLocation)
                .Where(si => si.StockLocation != null && !si.StockLocation.IsDeleted && si.StockLocation.Active)
                .AsQueryable();

            if (request.LocationId.HasValue)
                query = query.Where(si => si.StockLocationId == request.LocationId.Value);

            var items = await query.ToListAsync(cancellationToken);

            // Filter: Apply threshold comparison per location
            var results = items
                .Where(si =>
                {
                    var threshold = request.Threshold ?? si.StockLocation!.LowStockThreshold;
                    return si.CountOnHand <= threshold;
                })
                .Select(si => new Response
                {
                    Id = si.Id,
                    VariantId = si.VariantId,
                    StockLocationId = si.StockLocationId,
                    LocationName = si.StockLocation!.Name,
                    CountOnHand = si.CountOnHand,
                    Threshold = request.Threshold ?? si.StockLocation.LowStockThreshold,
                    Backorderable = si.Backorderable,
                    Status = si.CountOnHand == 0 ? "out_of_stock" : "low"
                })
                .ToList();

            // Map: Return low stock items
            return results;
        }
    }
}
