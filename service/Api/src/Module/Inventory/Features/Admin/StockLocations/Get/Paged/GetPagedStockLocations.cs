using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.Get.Paged;

/// <summary>Handles paged retrieval of stock locations.</summary>
public static partial class GetPagedStockLocations
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Executes the paged stock locations query.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of stock locations.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            // Parse: Validate and parse querying parameters for pagination, filtering, and sorting
            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Query: Retrieve stock locations, apply querying options, and map to paged result.
            var pagedResult = await dbContext.Set<StockLocation>()
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToListItem<Response>(),  cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}
