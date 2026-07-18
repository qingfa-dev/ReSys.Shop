using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    public sealed record Query : IQuery<List<Response>>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var items = await dbContext.Set<StockItem>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return items.Select(x => x.MapToListItem<Response>()).ToList();
        }
    }
}
