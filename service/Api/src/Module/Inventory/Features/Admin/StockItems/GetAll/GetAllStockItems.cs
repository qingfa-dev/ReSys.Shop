using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    public sealed record Query : IQuery<List<Response>>;

    /// <summary>Handler for getting all stock items.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Gets all stock items.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch all stock items without tracking for read-only access
            var items = await dbContext.Set<StockItem>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Transform: Map domain entities to response DTOs
            return items.Select(x => x.MapToListItem<Response>()).ToList();
        }
    }
}
