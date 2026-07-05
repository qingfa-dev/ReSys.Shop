using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Get.Paged;

/// <summary>Handles paged retrieval of stock items.</summary>
public static partial class GetPagedStockItems
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Executes the paged stock items query.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of stock items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            // Parse: Validate and parse querying parameters for pagination, filtering, and sorting
            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            // Query: Retrieve stock items, apply querying options, and map to paged result.
            var pagedResult = await dbContext.Set<StockItem>()
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToListItem<Response>(),cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}
