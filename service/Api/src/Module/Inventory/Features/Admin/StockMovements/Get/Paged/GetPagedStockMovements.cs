using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockMovements.Get.Paged;

/// <summary>Returns paged stock movements with optional date range, variant, and location filters.</summary>
public static partial class GetPagedStockMovements
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Applies filters and pagination to the stock movements query and returns paged results.</summary>
        /// <param name="request">The query containing paging and filter parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of stock movements.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            // Load: Retrieve stock movements with optional date/variant/location filters
            var query = dbContext.Set<StockMovement>().AsNoTracking().AsQueryable();

            if (parameters.FromUtc.HasValue)
                query = query.Where(m => m.CreatedAtUtc >= parameters.FromUtc.Value);
            if (parameters.ToUtc.HasValue)
                query = query.Where(m => m.CreatedAtUtc <= parameters.ToUtc.Value);
            if (parameters.VariantId.HasValue)
                query = query.Where(m => m.StockItem != null && m.StockItem.VariantId == parameters.VariantId.Value);
            if (parameters.StockLocationId.HasValue)
                query = query.Where(m => m.StockLocationId == parameters.StockLocationId.Value);

            // Parse: Validate and parse querying parameters for pagination, filtering, and sorting
            var parseAll = parameters.ParseAll(
                allowedFilterFields: StockMovementConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: StockMovementConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: StockMovementConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parseAll.IsFailure)
                return parseAll.Errors;

            var pagedResult = await query
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(model: parseAll.Value, projection: x => x.MapToListItem<Response>(), ct: cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}