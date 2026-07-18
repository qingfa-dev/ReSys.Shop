using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockTransfers.GetById;

public static partial class GetStockTransferById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<StockTransfer>()
                .AsNoTracking()
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (entity is null)
                return StockTransferResult.Failure.NotFound;

            return entity.MapToDetail<Response>();
        }
    }
}
