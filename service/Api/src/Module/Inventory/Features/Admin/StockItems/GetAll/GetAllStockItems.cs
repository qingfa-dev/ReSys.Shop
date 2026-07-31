using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Paging.Extensions;

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
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;

            // Load: Fetch stock items without tracking, ordered for stable paging
            return await dbContext.Set<StockItem>()
                .OrderBy(x => x.Id)
                .ToPagedOrAllAsync(x => x.MapToListItem<Response>(), pageModel, cancellationToken);
        }
    }
}
