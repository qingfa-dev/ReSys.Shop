using Shared.Operational.Persistence.Specifications.Sorting;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handler for getting all stock items.</summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Gets all stock items, paged or all in a single page.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate: Parse and validate query parameters against allowed fields
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: StockItemConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: StockItemConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: StockItemConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Fetch stock items without tracking, with querying and stable default sort
            return await dbContext.Set<StockItem>()
                .AsNoTracking()
                .ApplyQuerying(parsing.Value, defaultSortClauses: [new SortClause { Field = nameof(StockItem.Id) }])
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);
        }
    }
}
