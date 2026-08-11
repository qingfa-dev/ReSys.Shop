using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.GetById;

public static partial class GetStockItemById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>Handler for getting a stock item by ID.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets a stock item by ID.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch stock item by identifier without tracking
            var entity = await dbContext.Set<StockItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check: Return not-found if no stock item matches the requested ID
            if (entity is null)
                return StockItemResult.Errors.NotFound(request.Id);

            // Transform: Map domain entity to response DTO
            return entity.MapToDetail<Response>();
        }
    }
}
