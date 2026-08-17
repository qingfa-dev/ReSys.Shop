using Module.Inventory.Domain.StockItems;

namespace Module.Inventory.Features.Admin.StockItems.LowStock;

/// <summary>Handles retrieval of stock items below their location's low-stock threshold.</summary>
public static partial class GetLowStockItems
{
    public sealed record Query(Request Request, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Executes the get low stock items query.</summary>
        /// <param name="request">The query containing optional location and threshold filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of stock items below their threshold.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Load: Retrieve stock items with location filter
            var query = dbContext.Set<StockItem>()
                .Include(si => si.StockLocation)
                .Where(si => si.StockLocation != null && !si.StockLocation.IsDeleted && si.StockLocation.Active)
                .AsQueryable();

            if (request.Request.LocationId.HasValue)
                query = query.Where(si => si.StockLocationId == request.Request.LocationId.Value);

            var items = await query.ToListAsync(cancellationToken);

            // Filter: Apply threshold comparison per location
            var results = items
                .Where(si =>
                {
                    var threshold = request.Request.Threshold ?? si.StockLocation!.LowStockThreshold;
                    return si.CountOnHand <= threshold;
                })
                .Select(si => new Response
                {
                    Id = si.Id,
                    VariantId = si.VariantId,
                    StockLocationId = si.StockLocationId,
                    LocationName = si.StockLocation!.Name,
                    CountOnHand = si.CountOnHand,
                    Threshold = request.Request.Threshold ?? si.StockLocation!.LowStockThreshold,
                    Backorderable = si.Backorderable,
                    Status = si.CountOnHand == 0 ? LowStockStatus.OutOfStock : LowStockStatus.Low
                })
                .ToList();

            // Map: Return low stock items
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            var ordered = results.OrderBy(r => r.Id).ToList();

            // Transform: Return all in one page or honor caller-supplied paging
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(ordered, 1, Math.Max(1, ordered.Count), ordered.Count)
                : ordered.ToPagedResult(pageModel);
        }
    }
}