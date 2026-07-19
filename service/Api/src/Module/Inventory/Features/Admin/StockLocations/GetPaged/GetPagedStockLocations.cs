using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.GetPaged;

public static partial class GetPagedStockLocations
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handler for getting paged stock locations.</summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Gets a paged list of stock locations.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Validate: Parse query parameters against allowed filter, search, and sort fields
            var parseAll = parameters.ParseAll(
                allowedFilterFields: StockLocationConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: StockLocationConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: StockLocationConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Load: Apply querying parameters and fetch paged results without tracking
            var pagedResult = await dbContext.Set<StockLocation>()
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}