using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockTransfers.GetById;

public static partial class GetStockTransferById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>Handler for getting a stock transfer by ID.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets a stock transfer by ID.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the stock transfer with its line items, without tracking
            var entity = await dbContext.Set<StockTransfer>()
                .AsNoTracking()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            // Check: Return not-found if no transfer matches
            if (entity is null)
                return StockTransferResult.Failure.NotFound;

            // Transform: Map domain entity to response DTO
            return entity.MapToDetail<Response>();
        }
    }
}
